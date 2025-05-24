using log4net;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    /// <summary>
    /// 测试状态机实现类
    /// </summary>
    public class TestStateMachine : ITestStateMachine
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TestStateMachine));
        private readonly ISasTest _sasTest; // 测试接口，用于发送和接收测试指令
        private readonly IParameterService _parameterService; // 参数服务，用于加载和验证测试参数

        private TestState _currentState = TestState.Idle; // 空闲状态
        private CancellationTokenSource _cancellationTokenSource; // 取消令牌源，用于取消异步操作
        private TestParameters _parameters; // 测试参数对象
        private Timer _timeoutTimer; // 超时定时器
        private int _totalCycleCount = 0; // 总循环次数
        private int _successCount = 0; // 成功次数

        // 响应结果缓存
        private int _result;
        private int _state;
        private string _errorMessage;

        // 状态变化事件
        public event EventHandler<TestState> StateChanged;

        // 测试结果事件
        public event EventHandler<TestResult> TestResultReceived;

        // 测试计数变化事件
        public event EventHandler<TestCounterEventArgs> CounterChanged;

        // 当前状态
        public TestState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    _logger.Info($"状态变更: {_currentState}");
                    StateChanged?.Invoke(this, _currentState);
                }
            }
        }

        // 构造函数，依赖注入测试接口和参数服务
        public TestStateMachine(ISasTest sasTest, IParameterService parameterService)
        {
            _sasTest = sasTest ?? throw new ArgumentNullException(nameof(sasTest));
            _parameterService = parameterService ?? throw new ArgumentNullException(nameof(parameterService));
        }

        // 启动测试
        public async Task<bool> StartTestAsync()
        {
            if (CurrentState != TestState.Idle)
            {
                _logger.Warn("测试已经正在进行，无法启动新测试");
                return false;
            }

            try
            {
                // 加载参数
                _parameters = await _parameterService.LoadParameterAsync();
                if (!_parameterService.ValidateParameters(_parameters, out _errorMessage))
                {
                    _logger.Error($"参数验证失败: {_errorMessage}");
                    return false;
                }

                // 创建取消令牌
                _cancellationTokenSource = new CancellationTokenSource();

                // 启动状态机
                CurrentState = TestState.Initializing;

                // 开始异步状态机任务
                _ = RunStateMachineAsync(_cancellationTokenSource.Token);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"启动测试时发生错误: {ex.Message}", ex);
                CurrentState = TestState.Error;
                return false;
            }
        }

        // 停止测试
        public async Task<bool> StopTestAsync()
        {
            if (CurrentState == TestState.Idle)
            {
                _logger.Info("测试已处于空闲状态，无需停止");
                return true;
            }

            try
            {
                CurrentState = TestState.Stopping;

                // 取消正在进行的任务
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                // 停止超时定时器
                StopTimeoutTimer();

                // 发送停止指令
                var stopData = DataFactory.CreateData(FunctionType.Stop);
                string response = _sasTest.ReadData(stopData);
                _logger.Info($"发送停止指令，响应: {response}");

                // 切换到空闲状态
                CurrentState = TestState.Idle;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"停止测试失败: {ex.Message}", ex);
                CurrentState = TestState.Error;
                return false;
            }
        }

        // 复位功能 - 清除状态和计数器
        public async Task<bool> ResetAsync()
        {
            try
            {
                // 如果测试正在运行，先停止测试
                if (CurrentState != TestState.Idle && CurrentState != TestState.Error)
                {
                    await StopTestAsync();
                }

                _logger.Info("状态机复位");

                // 清除计数器
                _totalCycleCount = 0;
                _successCount = 0;

                // 重置内部状态
                _result = 0;
                _state = 0;
                _errorMessage = string.Empty;

                // 如果处于错误状态，恢复到空闲状态
                if (CurrentState == TestState.Error)
                {
                    CurrentState = TestState.Idle;
                }

                // 清除锁付信息指令
                var clearData = DataFactory.CreateData(FunctionType.TightenInfoControl);
                string response = _sasTest.ReadData(clearData);
                _logger.Info($"发送清除锁付信息指令，响应: {response}");

                // 触发计数器事件，通知订阅者
                OnCounterChanged();

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"复位失败: {ex.Message}", ex);
                return false;
            }
        }

        // 状态机主循环
        private async Task RunStateMachineAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    switch (CurrentState)
                    {
                        case TestState.Initializing:
                            await InitializeTestAsync();
                            break;

                        case TestState.ForwardDelay:
                            await Task.Delay(TimeSpan.FromSeconds(_parameters.ForwardDelay), cancellationToken);
                            CurrentState = TestState.Forward;
                            break;

                        case TestState.Forward:
                            StartForwardRotation();
                            CurrentState = TestState.ForwardWaiting;
                            StartTimeoutTimer((int)(_parameters.Timeout * 1000));
                            break;

                        case TestState.ForwardWaiting:
                            // 等待响应由消息处理部分处理
                            await Task.Delay(100, cancellationToken); // 小延迟避免CPU高占用
                            break;

                        case TestState.RotationInterval:
                            await Task.Delay(TimeSpan.FromSeconds(_parameters.RotationInterval), cancellationToken);
                            CurrentState = TestState.ReverseDelay;
                            break;

                        case TestState.ReverseDelay:
                            await Task.Delay(TimeSpan.FromSeconds(_parameters.ReverseDelay), cancellationToken);
                            CurrentState = TestState.Reverse;
                            break;

                        case TestState.Reverse:
                            StartReverseRotation();
                            CurrentState = TestState.ReverseWaiting;
                            StartTimeoutTimer((int)(_parameters.Timeout * 1000));
                            break;

                        case TestState.ReverseWaiting:
                            // 等待响应由消息处理部分处理
                            await Task.Delay(100, cancellationToken); // 小延迟避免CPU高占用
                            break;

                        case TestState.StartupInterval:
                            // 增加循环计数
                            _totalCycleCount++;
                            OnCounterChanged();

                            await Task.Delay(TimeSpan.FromSeconds(_parameters.StartupInterval), cancellationToken);
                            CurrentState = TestState.ForwardDelay;
                            break;

                        case TestState.Stopping:
                        case TestState.Error:
                            return; // 退出循环

                        default:
                            await Task.Delay(100, cancellationToken);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不需要处理
                _logger.Info("测试已取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"状态机执行错误: {ex.Message}", ex);
                CurrentState = TestState.Error;
            }
            finally
            {
                StopTimeoutTimer();
                if (CurrentState != TestState.Idle)
                {
                    CurrentState = TestState.Idle;
                }
            }
        }

        // 初始化测试
        private async Task InitializeTestAsync()
        {
            try
            {
                // 发送101订阅指令
                var subscribeData = DataFactory.CreateData(FunctionType.Subcribe);
                string response = _sasTest.ReadData(subscribeData);
                _logger.Info($"发送订阅指令，响应: {response}");

                // 发送123锁付模式指令
                var lockModeData = DataFactory.CreateData(FunctionType.LockMode);
                response = _sasTest.ReadData(lockModeData);
                _logger.Info($"发送锁付模式指令，响应: {response}");

                // 初始化完成，进入正转延时状态
                CurrentState = TestState.ForwardDelay;
            }
            catch (Exception ex)
            {
                _logger.Error($"初始化测试失败: {ex.Message}", ex);
                CurrentState = TestState.Error;
                throw;
            }
        }

        // 开始正转操作
        private void StartForwardRotation()
        {
            try
            {
                var lockScrewData = DataFactory.CreateData(FunctionType.LockScrewAction);
                string response = _sasTest.ReadData(lockScrewData);
                _logger.Info($"发送锁螺丝指令，响应: {response}");
            }
            catch (Exception ex)
            {
                _logger.Error($"启动正转操作失败: {ex.Message}", ex);
                CurrentState = TestState.Error;
            }
        }

        // 开始反转操作
        private void StartReverseRotation()
        {
            try
            {
                var removeScrewData = DataFactory.CreateData(FunctionType.RemoveScrewAction);
                string response = _sasTest.ReadData(removeScrewData);
                _logger.Info($"发送拆螺丝指令，响应: {response}");
            }
            catch (Exception ex)
            {
                _logger.Error($"启动反转操作失败: {ex.Message}", ex);
                CurrentState = TestState.Error;
            }
        }

        // 发送停止指令
        private void SendStopCommand()
        {
            try
            {
                var stopData = DataFactory.CreateData(FunctionType.Stop);
                string response = _sasTest.ReadData(stopData);
                _logger.Info($"发送停止指令，响应: {response}");
            }
            catch (Exception ex)
            {
                _logger.Error($"发送停止指令失败: {ex.Message}", ex);
            }
        }

        // 发送清除锁付信息指令
        private void SendClearTightenInfoCommand()
        {
            try
            {
                var clearData = DataFactory.CreateData(FunctionType.TightenInfoControl);
                string response = _sasTest.ReadData(clearData);
                _logger.Info($"发送清除锁付信息指令，响应: {response}");
            }
            catch (Exception ex)
            {
                _logger.Error($"发送清除锁付信息指令失败: {ex.Message}", ex);
            }
        }

        // 启动超时计时器
        private void StartTimeoutTimer(int milliseconds)
        {
            StopTimeoutTimer();
            _timeoutTimer = new Timer(OnTimeout, null, milliseconds, Timeout.Infinite);
        }

        // 停止超时计时器
        private void StopTimeoutTimer()
        {
            if (_timeoutTimer != null)
            {
                _timeoutTimer.Dispose();
                _timeoutTimer = null;
            }
        }

        // 超时处理
        private void OnTimeout(object state)
        {
            try
            {
                _logger.Warn($"当前状态 {CurrentState} 操作超时");

                // 根据当前状态处理超时
                if (CurrentState == TestState.ForwardWaiting)
                {
                    // 发送停止指令
                    SendStopCommand();

                    // 通知测试结果
                    var result = new TestResult
                    {
                        IsSuccess = false,
                        Message = "正转操作超时",
                        ErrorCode = -1
                    };
                    TestResultReceived?.Invoke(this, result);

                    // 继续到下一个状态
                    CurrentState = TestState.RotationInterval;
                }
                else if (CurrentState == TestState.ReverseWaiting)
                {
                    // 发送停止指令
                    SendStopCommand();

                    // 通知测试结果
                    var result = new TestResult
                    {
                        IsSuccess = false,
                        Message = "反转操作超时",
                        ErrorCode = -1
                    };
                    TestResultReceived?.Invoke(this, result);

                    // 继续到下一个状态
                    CurrentState = TestState.StartupInterval;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理超时发生错误: {ex.Message}", ex);
                CurrentState = TestState.Error;
            }
        }

        // 处理接收到的消息
        public void HandleMessage(string message)
        {
            try
            {
                // 解析收到的JSON消息
                var response = JsonConvert.DeserializeObject<dynamic>(message);

                // 检查是否为锁付结果回复
                if (response.reply == 203)
                {
                    // 提取关键字段
                    _state = (int)response.state;
                    _result = (int)response.result;

                    // 处理正转等待状态的响应
                    if (CurrentState == TestState.ForwardWaiting)
                    {
                        HandleForwardResponse();
                    }
                    // 处理反转等待状态的响应
                    else if (CurrentState == TestState.ReverseWaiting)
                    {
                        HandleReverseResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理消息失败: {ex.Message}", ex);
            }
        }

        // 处理正转响应
        private void HandleForwardResponse()
        {
            // 检查state和result
            if (_state == 0) // 锁付已停止
            {
                StopTimeoutTimer();

                bool isSuccess = _result == 0;
                string message = isSuccess ? "正转操作成功" : $"正转操作失败，错误码：{_result}";

                if (isSuccess)
                {
                    _successCount++;
                    OnCounterChanged();
                }

                // 通知测试结果
                var result = new TestResult
                {
                    IsSuccess = isSuccess,
                    Message = message,
                    ErrorCode = _result
                };
                TestResultReceived?.Invoke(this, result);

                // 进入下一个状态
                CurrentState = TestState.RotationInterval;
            }
        }

        // 处理反转响应
        private void HandleReverseResponse()
        {
            // 检查state和result (简化处理，可以根据实际需求调整)
            StopTimeoutTimer();

            // 假设反转操作成功
            bool isSuccess = true;
            _successCount++;
            OnCounterChanged();

            // 通知测试结果
            var result = new TestResult
            {
                IsSuccess = isSuccess,
                Message = "反转操作成功",
                ErrorCode = 0
            };
            TestResultReceived?.Invoke(this, result);

            // 进入下一个状态
            CurrentState = TestState.StartupInterval;
        }

        // 处理错误
        private void HandleError(string errorMessage, int errorCode = -1)
        {
            _logger.Error($"错误发生: {errorMessage} (错误代码: {errorCode})");

            // 通知订阅者错误信息
            var result = new TestResult
            {
                IsSuccess = false,
                Message = errorMessage,
                ErrorCode = errorCode
            };
            TestResultReceived?.Invoke(this, result);

            // 切换到错误状态
            CurrentState = TestState.Error;
        }

        // 触发计数器变更事件
        private void OnCounterChanged()
        {
            CounterChanged?.Invoke(this, new TestCounterEventArgs
            {
                TotalCycles = _totalCycleCount,
                SuccessCount = _successCount,
            });
        }

        // 重置计数器
        public void ResetCounters()
        {
            _totalCycleCount = 0;
            _successCount = 0;
            OnCounterChanged();
        }
    }
}

