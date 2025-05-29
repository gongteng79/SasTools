using log4net;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Interface;
using SasTools.States;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SasTools.Services;
using System.Text.Json;


namespace SasTools.Domain
{
    // 测试状态机实现类
    public class TestStateMachine : ITestStateMachine
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TestStateMachine));
        private readonly ICommandService _commandService; // 命令服务，用于发送命令
        private readonly IParameterService _parameterService; // 参数服务，用于加载和验证测试参数
        private readonly IStateFactory _stateFactory; // 状态工厂，用于创建状态对象
        private TestState _state = TestState.Idle;
        private IState _currentStateObj; // 当前状态对象
        private TestState _currentState = TestState.Idle; // 当前状态枚举
        private CancellationTokenSource _cancellationTokenSource; // 取消令牌源，用于取消异步操作
        private Timer _timeoutTimer; // 超时定时器
        private bool _isRunning = false;
        private bool _isPause = false;

        // 状态机上下文
        private readonly TestStateMachineContext _context;

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
            private set
            {
                if (_currentState != value)
                {
                    // 先让当前状态执行退出逻辑
                    _currentStateObj?.Exit(this);

                    var oldState = _currentState;
                    _currentState = value;
                    _logger.Info($"状态变更: {oldState} -> {_currentState}");

                    // 创建新状态对象
                    _currentStateObj = _stateFactory.CreateState(_currentState);

                    // 调用新状态的进入逻辑
                    _currentStateObj?.Enter(this);

                    // 触发状态变更事件
                    StateChanged?.Invoke(this, _currentState);
                }
            }
        }

        #region 构造函数
        // 构造函数 - 使用依赖注入
        public TestStateMachine(ICommandService commandService, IParameterService parameterService, IStateFactory stateFactory)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            //_parameterService = parameterService ?? throw new ArgumentNullException(nameof(parameterService));
            _stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));

            // 初始化上下文
            _context = new TestStateMachineContext();

            // 初始化当前状态对象
            _currentStateObj = _stateFactory.CreateState(_currentState);
        }

        //构造函数
        public TestStateMachine(ISasTest sasTest, IParameterService parameterService)
            : this(new CommandService(sasTest), parameterService, new DefaultStateFactory())
        {
        }
        #endregion

        #region 公共方法
        // 启动测试
        public async Task<bool> StartTestAsync()
        {
            _isRunning = true;
            _state = TestState.Idle;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => RunStateMachineAsync(_cancellationTokenSource.Token));

            return true;
            //if (CurrentState != TestState.Idle)
            //{
            //    _logger.Warn("测试已经正在进行，无法启动新测试");
            //    return false;
            //}

            //try
            //{
            //    // 加载参数
            //    _context.Parameters = await _parameterService.LoadParameterAsync();
            //    string errorMessage;
            //    if (!_parameterService.ValidateParameters(_context.Parameters, out errorMessage))
            //    {
            //        _logger.Error($"参数验证失败: {errorMessage}");
            //        NotifyError(errorMessage, -1);
            //        return false;
            //    }

            //    // 重置计数器
            //    _context.Reset();

            //    // 创建取消令牌
            //    _cancellationTokenSource = new CancellationTokenSource();

            //    // 切换到初始化状态
            //    CurrentState = TestState.Initializing;

            //    // 启动状态机主循环
            //    _ = RunStateMachineAsync(_cancellationTokenSource.Token);

            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error($"启动测试时发生错误: {ex.Message}", ex);
            //    CurrentState = TestState.Error;
            //    return false;
            //}
        }

        // 停止测试
        public async Task<bool> StopTestAsync()
        {
            _isRunning = false;
            _state = TestState.Idle;
            //    取消正在进行的任务
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            await _commandService.ExecuteCommand(CommandType.Stop);
            return true;

            //if (CurrentState == TestState.Idle)
            //{
            //    _logger.Info("测试已处于空闲状态，无需停止");
            //    return true;
            //}

            //try
            //{
            //    CurrentState = TestState.Stopping;

            //    取消正在进行的任务
            //    if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            //    {
            //        _cancellationTokenSource.Cancel();
            //    }

            //    停止超时定时器
            //    StopTimeoutTimer();

            //    发送停止指令
            //   await _commandService.ExecuteCommandAsync(CommandType.Stop);

            //    切换到空闲状态
            //   CurrentState = TestState.Idle;
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error($"停止测试失败: {ex.Message}", ex);
            //    CurrentState = TestState.Error;
            //    return false;
            //}
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

                // 重置上下文
                _context.Reset();

                // 如果处于错误状态，恢复到空闲状态
                if (CurrentState == TestState.Error)
                {
                    CurrentState = TestState.Idle;
                }

                // 通知计数器变更
                OnCounterChanged();

                // 发送清除锁付信息指令
                await _commandService.ExecuteCommand(CommandType.ClearTightenInfo);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"复位失败: {ex.Message}", ex);
                return false;
            }
        }

        // 处理接收到的消息
        public void HandleMessage(string message)
        {
            try
            {
                // 解析收到的JSON消息
                var response = JsonConvert.DeserializeObject<dynamic>(message);

                // 保存到上下文
                _context.ResponseMessage = message;

                // 检查是否为锁付结果回复
                if (response.reply == 203)
                {
                    // 提取关键字段并保存到上下文
                    _context.State = (int)response.state;
                    _context.Result = (int)response.result;

                    // 让当前状态处理消息
                    _currentStateObj?.HandleMessage(this, message);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理消息失败: {ex.Message}", ex);
            }
        }

        // 重置计数器
        public void ResetCounters()
        {
            _context.Reset();
            OnCounterChanged();
        }
        #endregion

        #region 内部方法
        // 状态机主循环
        private void RunStateMachineAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_currentStateObj == null)
                    {
                        Task.Delay(100, cancellationToken);
                        continue;
                    }
                    if (!_isRunning)
                    {
                        return;
                    }

                    TestLockingScrewsAsync();

                    //// 让当前状态处理逻辑
                    //TestState nextState = await _currentStateObj.ProcessAsync(this, cancellationToken);

                    //// 如果需要转换状态
                    //if (nextState != CurrentState)
                    //{
                    //    CurrentState = nextState;
                    //}
                    //else
                    //{
                    //    // 避免CPU高占用
                    //    await Task.Delay(50, cancellationToken);
                    //}

                    //// 如果是终止状态，退出循环
                    //if (CurrentState == TestState.Idle || CurrentState == TestState.Error)
                    //{
                    //    break;
                    //}
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不需要处理
                _logger.Info("状态机执行已取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"状态机执行错误: {ex.Message}", ex);
                CurrentState = TestState.Error;
            }
            finally
            {
                StopTimeoutTimer();

                // 确保最终回到空闲状态
                if (CurrentState != TestState.Idle && CurrentState != TestState.Error)
                {
                    CurrentState = TestState.Idle;
                }
            }
        }

        // 通知测试结果
        internal void NotifyTestResult(bool isSuccess, string message, int errorCode)
        {
            var result = new TestResult
            {
                IsSuccess = isSuccess,
                Message = message,
                ErrorCode = errorCode
            };

            TestResultReceived?.Invoke(this, result);
        }

        // 通知错误
        internal void NotifyError(string errorMessage, int errorCode)
        {
            _logger.Error($"错误: {errorMessage} (错误代码: {errorCode})");
            NotifyTestResult(false, errorMessage, errorCode);
        }

        // 启动超时计时器
        internal void StartTimeoutTimer(int milliseconds)
        {
            StopTimeoutTimer();
            _timeoutTimer = new Timer(OnTimeout, null, milliseconds, Timeout.Infinite);
        }

        // 停止超时计时器
        internal void StopTimeoutTimer()
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

                // 让当前状态处理超时
                _currentStateObj?.HandleTimeout(this);
            }
            catch (Exception ex)
            {
                _logger.Error($"处理超时发生错误: {ex.Message}", ex);
                CurrentState = TestState.Error;
            }
        }

        // 增加成功计数
        internal void IncrementSuccessCount()
        {
            _context.SuccessCount++;
            OnCounterChanged();
        }

        // 增加循环计数
        internal void IncrementCycleCount()
        {
            _context.TotalCycleCount++;
            OnCounterChanged();
        }

        // 触发计数器变更事件
        private void OnCounterChanged()
        {
            CounterChanged?.Invoke(this, new TestCounterEventArgs
            {
                TotalCycles = _context.TotalCycleCount,
                SuccessCount = _context.SuccessCount
            });
        }

        // 获取上下文
        internal TestStateMachineContext GetContext()
        {
            return _context;
        }

        // 获取命令服务
        internal ICommandService GetCommandService()
        {
            return _commandService;
        }
        #endregion

        private async Task TestLockingScrewsAsync()
        {
            switch (_state)
            {
                case TestState.Idle:
                    _state = TestState.Initializing;
                    break;

                case TestState.Initializing:
                    await SubscribeScriewMode();
                    _state = TestState.ForwardDelay;
                    break;

                case TestState.ForwardDelay:
                    await Task.Delay(2000);
                    _state = TestState.Forward;
                    break;

                case TestState.Forward:
                    var forwardResult = await _commandService.ExecuteCommand(CommandType.Forward);
                    if (CheckLockStatus(forwardResult))
                    {
                        _state = TestState.InputScrewData;
                    }
                    break;

                case TestState.InputScrewData:
                    await _commandService.ExecuteCommand(CommandType.InputScrewData);
                    _state = TestState.ForwardWaiting;
                    break;

                case TestState.ForwardWaiting:
                    await Task.Delay(1000);
                    _state = TestState.ReverseDelay;
                    break;

                case TestState.ReverseDelay:
                    await Task.Delay(1000);
                    _state = TestState.Reverse;
                    break;

                case TestState.Reverse:
                    var reverseResult = await _commandService.ExecuteCommand(CommandType.Reverse);
                    if (CheckLockStatus(reverseResult))
                    {
                        _state = TestState.ReverseWaiting;
                    }
                    break;

                case TestState.ReverseWaiting:
                    await Task.Delay(1000);
                    _state = TestState.StartupInterval;
                    break;

                case TestState.StartupInterval:
                    await Task.Delay(1000);
                    _state = TestState.ForwardDelay; // 循环回到 ForwardDelay
                    break;

                case TestState.Error:
                    _logger.Error("状态机进入错误状态");
                    break;
            }
        }


        private async Task SubscribeScriewMode()
        {
            await _commandService.ExecuteCommand(CommandType.Subscribe);
        }

        private bool CheckLockStatus(string jsonMsg)
        {
            //jsonMsg = jsonMsg.Replace("\\\"", "\"");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            //LockResult lockResult = System.Text.Json.JsonSerializer.Deserialize<LockResult>(jsonMsg, options);

            try
            {
                var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                // 检查是否为锁付结果回复
                if (response.reply == 203)
                {
                    int state = (int)response.state;
                    int result = (int)response.result;

                    // 如果锁付已停止，进行结果判断
                    if (state == 0 && result == 0)
                    {
                        //if (lockResult.Result == 0 && lockResult.State == 0)
                        //{
                        //    return true;
                        //}
                        //else
                        //{
                        //    return false;
                        //}
                        return true;
                    }
                    else
                    {
                        this._commandService.ExecuteCommand(CommandType.Stop);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理消息失败: {ex.Message}", ex);
                return false;
            }
        }
    }
}



