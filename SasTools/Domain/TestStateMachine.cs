using log4net;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SasTools.Services;
using WpFramework.EventBus;
using SasTools.Events;
using SasTools.Models;

namespace SasTools.Domain
{
    // 测试状态机实现类
    public class TestStateMachine
    {
        #region 常量定义
        public static class Constants
        {
            public const int DEFAULT_POLLING_INTERVAL = 50;//默认轮询间隔
            public const int DEFAULT_RETRY_DELAY = 100;//重试延时
            public const int MAX_COMMUNICATION_RETRIES = 3;//最大通信重试次数
            public const int STATE_TRANSITION_DELAY = 10;//状态转换延时
            public const int DEVICE_STOP_WAIT_TIME = 500;//设备停止等待时间
            public const int DEVICE_CLEAR_WAIT_TIME = 300;//设备清理等待时间
            public const int CANCELLATION_WAIT_TIME = 100;//取消操作等待时间
        }

        public static class RecoveryTimes
        {
            public const int IDLE_SPIN_RECOVERY = 3000;//空转恢复时间
            public const int TORQUE_REACHED_RECOVERY = 2000;//扭力到达恢复时间
            public const int TIMEOUT_RECOVERY = 2500;//超时恢复时间
            public const int MOTOR_ERROR_RECOVERY = 4000;//电机错误恢复时间
            public const int STANDARD_RECOVERY = 2000;//标准恢复时间
            public const int EXTRA_STABILITY_DELAY = 1500;//额外稳定延时
        }
        #endregion

        #region 字段
        private readonly ILog _logger = LogManager.GetLogger(typeof(TestStateMachine));
        private readonly FatigueParams _parameter;
        private readonly IEventBus _eventBus;
        private readonly IDevice device;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private bool _isPause = false;

        // 状态相关
        private TestState _state = TestState.Idle;
        private string _machineMessage = "";

        // 计数器相关
        private int _totalCycles = 0;
        private int _successfulCycles = 0;
        private bool _forwardSuccess = false;
        private bool _reverseSuccess = false;

        // 错误处理相关
        private string _lastErrorMessage = "";
        private int _lastErrorResult = 0;
        #endregion

        #region 构造函数
        public TestStateMachine(IDevice device, FatigueParams parameter, IEventBus eventBus)
        {
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter), "参数服务不能为空");
            this.device = device ?? throw new ArgumentNullException(nameof(device), "设备接口不能为空");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "事件总线不能为空");
        }
        #endregion

        #region 公共方法
        public bool StartTest()
        {
            if (_isRunning) return false;

            _isRunning = true;
            _state = TestState.Idle;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await RunStateMachineAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    _logger.Error($"状态机执行异常: {ex.Message}", ex);
                }
            });

            _logger.Info("测试已启动");
            return true;
        }

        public bool StopTest()
        {
            if (!_isRunning) return false;

            _isRunning = false;
            _state = TestState.Idle;
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            try
            {
                device.ExecuteCommand(SasCommandType.Stop);
                _logger.Info("测试已停止");
            }
            catch (Exception ex)
            {
                _logger.Error($"停止设备时出错: {ex.Message}", ex);
            }
            return true;
        }

        public void ResetCounters()
        {
            _totalCycles = 0;
            _successfulCycles = 0;
            _forwardSuccess = false;
            _reverseSuccess = false;
            _lastErrorMessage = "";
            _lastErrorResult = 0;
            PublishCounterUpdate();
            _logger.Info("计数器已重置");
        }

        public void Reset()
        {
            try
            {
                StopTest();
                Task.Delay(Constants.CANCELLATION_WAIT_TIME).Wait();

                _state = TestState.Idle;
                _machineMessage = "系统已复位";
                _isRunning = false;
                _isPause = false;

                _totalCycles = 0;
                _successfulCycles = 0;
                _forwardSuccess = false;
                _reverseSuccess = false;
                _lastErrorMessage = "";
                _lastErrorResult = 0;

                // 在后台线程执行设备清理操作，避免阻塞UI
                Task.Run(async () =>
                {
                    try
                    {
                        // 多次停止命令确保设备完全停止
                        device.ExecuteCommand(SasCommandType.Stop);
                        await Task.Delay(Constants.DEVICE_STOP_WAIT_TIME);

                        device.ExecuteCommand(SasCommandType.Stop);
                        await Task.Delay(200);

                        // 多次清理设备错误信息，确保彻底清除
                        device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                        await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME);

                        device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                        await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME);

                        // 重新订阅前再次确保设备状态清洁
                        device.ExecuteCommand(SasCommandType.Subscribe);
                        await Task.Delay(500); // 增加订阅后的等待时间

                        // 验证设备状态是否已清理干净
                        await VerifyDeviceStateCleared();

                        _logger.Info("设备状态已彻底清理并重新初始化");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"复位时清理设备状态失败: {ex.Message}");
                    }
                });

                PublishStateUpdate(MachineStatusType.Idle);
                PublishCounterUpdate();
                _logger.Info("状态机已完全重置");
            }
            catch (Exception ex)
            {
                _logger.Error($"重置状态机失败: {ex.Message}", ex);
                throw;
            }
        }
        #endregion

        #region 私有方法
        private async Task RunStateMachineAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info("状态机开始运行");
                while (!cancellationToken.IsCancellationRequested && _isRunning)
                {
                    if (_isPause)
                    {
                        await Task.Delay(Constants.CANCELLATION_WAIT_TIME, cancellationToken);
                        continue;
                    }

                    await TestLockingScrewsAsync();
                    await Task.Delay(Constants.STATE_TRANSITION_DELAY, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("状态机执行已取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"状态机执行错误: {ex.Message}", ex);
                _state = TestState.Error;
                _eventBus.Publish(new RefreshMachineState("执行出错: " + ex.Message, _state.ToString(), MachineStatusType.Error));
            }
            finally
            {
                if (_isRunning)
                {
                    _isRunning = false;
                    try
                    {
                        device.ExecuteCommand(SasCommandType.Stop);
                        _logger.Info("状态机结束时已停止设备");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"状态机结束时停止设备失败: {ex.Message}");
                    }
                }
                _logger.Info("状态机已结束运行");
            }
        }

        private async Task TestLockingScrewsAsync()
        {
            try
            {
                if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                bool shouldUpdateDisplay = true; // 控制是否在方法末尾更新显示

                switch (_state)
                {
                    case TestState.Idle:
                        _state = TestState.Initializing;
                        break;

                    case TestState.Initializing:
                        await InitializeAndVerifyDeviceState();
                        _state = TestState.ForwardDelay;
                        _machineMessage = "初始化完成";
                        break;

                    case TestState.ForwardDelay:
                        _machineMessage = "正转延时中...";
                        PublishStateUpdate(MachineStatusType.Waiting);
                        shouldUpdateDisplay = false; //已经发布了状态更新

                        try
                        {
                            await Task.Delay(_parameter.ForwardDelay, _cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }

                        if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        _state = TestState.Forward;
                        break;

                    case TestState.Forward:
                        _machineMessage = "执行正转...";
                        PublishStateUpdate(MachineStatusType.Forward);

                        // 在执行正转前，先清理可能的残留状态
                        await ClearDeviceStateBeforeForward();

                        device.ExecuteCommand(SasCommandType.Forward);
                        var result = await CheckLockStatusAsync();

                        if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (result.success)
                        {
                            _logger.Info("正转锁付成功");
                            _forwardSuccess = true;
                            _state = TestState.ReverseDelay;
                            _machineMessage = "正转OK";
                            PublishStateUpdate(MachineStatusType.Forward);
                            shouldUpdateDisplay = false; // 已经发布了状态更新，避免重复
                        }
                        else
                        {
                            if (IsUserStoppedError(result.errorMessage))
                            {
                                _logger.Info("正转被用户停止");
                                return;
                            }

                            _logger.Error($"正转锁付失败: state={result.state}, result={result.result}, 错误: {result.errorMessage}");

                            _lastErrorMessage = result.errorMessage;
                            _lastErrorResult = result.result;

                            _state = TestState.Error;
                            _machineMessage = $"正转失败: {result.errorMessage}";
                            PublishStateUpdate(MachineStatusType.Error);
                            device.ExecuteCommand(SasCommandType.Stop);

                            _totalCycles++;
                            PublishCounterUpdate();

                            if (ShouldStopTest())
                            {
                                _isRunning = false;
                            }
                            shouldUpdateDisplay = false; // 已经发布了状态更新
                        }
                        break;

                    case TestState.ReverseDelay:
                        _machineMessage = "反转延时中...";
                        PublishStateUpdate(MachineStatusType.Waiting);
                        shouldUpdateDisplay = false; // 已经发布了状态更新

                        try
                        {
                            await Task.Delay(_parameter.ReverseDelay, _cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }

                        if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        _state = TestState.Reverse;
                        break;

                    case TestState.Reverse:
                        _machineMessage = "执行反转...";
                        PublishStateUpdate(MachineStatusType.Reverse);
                        shouldUpdateDisplay = false; // 已经发布了状态更新

                        device.ExecuteCommand(SasCommandType.Reverse);

                        try
                        {
                            await Task.Delay(2000, _cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }

                        if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        _reverseSuccess = true;
                        _state = TestState.Stopping;
                        _machineMessage = "反转OK";
                        break;

                    case TestState.Stopping:
                        device.ExecuteCommand(SasCommandType.Stop);
                        UpdateCounters();

                        if (ShouldStopTest())
                        {
                            return;
                        }

                        _state = TestState.ForwardDelay;
                        _machineMessage = "循环完成，准备下一次测试";
                        break;

                    case TestState.Error:
                        await HandleErrorStateAsync();
                        shouldUpdateDisplay = false; // HandleErrorStateAsync 内部会处理状态更新
                        break;

                    default:
                        _logger.Warn($"未处理的状态: {_state}");
                        _state = TestState.Idle;
                        break;
                }

                // 只有在没有明确发布状态更新的情况下才调用 UpdateStatusDisplay
                if (shouldUpdateDisplay)
                {
                    UpdateStatusDisplay();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"状态处理错误: {ex.Message}", ex);
                _state = TestState.Error;
                _machineMessage = $"处理错误: {ex.Message}";
                PublishStateUpdate(MachineStatusType.Error);
            }
        }

        private async Task HandleErrorStateAsync()
        {
            _machineMessage = "错误状态，分析处理中";
            PublishStateUpdate(MachineStatusType.Error);
            device.ExecuteCommand(SasCommandType.Stop);

            if (ShouldStopTest())
            {
                _logger.Info("已达到NG次数限制，停止测试");
                _isRunning = false;
                return;
            }

            var strategy = DetermineRecoveryStrategy(_lastErrorResult, _lastErrorMessage);
            _logger.Info($"错误类型: {_lastErrorMessage}, 采用恢复策略: {strategy.Name}");

            await ExecuteRecoveryStrategyAsync(strategy);

            if (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _state = TestState.ForwardDelay;
                _machineMessage = $"错误恢复完成，准备重试 - {strategy.Name}";
            }
        }

        private ErrorRecoveryStrategy DetermineRecoveryStrategy(int errorResult, string errorMessage)
        {
            // 空转错误
            if ((errorResult & (1 << 29)) != 0)
            {
                return new ErrorRecoveryStrategy
                {
                    Name = "空转恢复",
                    WaitTime = RecoveryTimes.IDLE_SPIN_RECOVERY,
                    RequiresClearTightenInfo = true,
                    RequiresResubscribe = true,
                    RequiresExtraDelay = true
                };
            }

            // 扭力到达错误
            if ((errorResult & (1 << 6)) != 0)
            {
                return new ErrorRecoveryStrategy
                {
                    Name = "扭力到达恢复",
                    WaitTime = RecoveryTimes.TORQUE_REACHED_RECOVERY,
                    RequiresClearTightenInfo = true,
                    RequiresResubscribe = false,
                    RequiresExtraDelay = true
                };
            }

            // 超时错误
            if ((errorResult & (1 << 3)) != 0 || (errorMessage != null && errorMessage.Contains("超时")))
            {
                return new ErrorRecoveryStrategy
                {
                    Name = "超时恢复",
                    WaitTime = RecoveryTimes.TIMEOUT_RECOVERY,
                    RequiresClearTightenInfo = true,
                    RequiresResubscribe = true,
                    RequiresExtraDelay = false
                };
            }

            // 电机相关错误
            if ((errorResult & (1 << 0)) != 0 || (errorResult & (1 << 4)) != 0)
            {
                return new ErrorRecoveryStrategy
                {
                    Name = "电机错误恢复",
                    WaitTime = RecoveryTimes.MOTOR_ERROR_RECOVERY,
                    RequiresClearTightenInfo = true,
                    RequiresResubscribe = true,
                    RequiresExtraDelay = true
                };
            }

            // 默认恢复策略
            return new ErrorRecoveryStrategy
            {
                Name = "标准恢复",
                WaitTime = RecoveryTimes.STANDARD_RECOVERY,
                RequiresClearTightenInfo = true,
                RequiresResubscribe = false,
                RequiresExtraDelay = false
            };
        }

        private async Task ExecuteRecoveryStrategyAsync(ErrorRecoveryStrategy strategy)
        {
            try
            {
                _machineMessage = $"执行{strategy.Name}，等待{strategy.WaitTime}ms";
                await Task.Delay(strategy.WaitTime, _cancellationTokenSource.Token);

                if (strategy.RequiresClearTightenInfo)
                {
                    device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                    _logger.Info("已清理设备错误状态");
                    await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME, _cancellationTokenSource.Token);
                }

                if (strategy.RequiresResubscribe)
                {
                    device.ExecuteCommand(SasCommandType.Subscribe);
                    _logger.Info("已重新订阅设备");
                    await Task.Delay(300, _cancellationTokenSource.Token);
                }

                if (strategy.RequiresExtraDelay)
                {
                    _machineMessage = "额外稳定延时中...";
                    await Task.Delay(RecoveryTimes.EXTRA_STABILITY_DELAY, _cancellationTokenSource.Token);
                }

                _logger.Info($"恢复策略 {strategy.Name} 执行完成");
            }
            catch (OperationCanceledException)
            {
                _logger.Info("恢复策略执行被取消");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warn($"执行恢复策略失败: {ex.Message}");
            }
        }

        private void UpdateStatusDisplay()
        {
            MachineStatusType statusType;

            switch (_state)
            {
                case TestState.Forward:
                    statusType = MachineStatusType.Forward;
                    break;
                case TestState.Reverse:
                    statusType = MachineStatusType.Reverse;
                    break;
                case TestState.ForwardDelay:
                    statusType = MachineStatusType.Waiting;
                    break;
                case TestState.ReverseDelay:
                    statusType = MachineStatusType.Waiting;
                    break;
                case TestState.Error:
                    statusType = MachineStatusType.Error;
                    break;
                case TestState.Idle:
                    statusType = MachineStatusType.Idle;
                    break;
                default:
                    statusType = MachineStatusType.Normal;
                    break;
            }

            PublishStateUpdate(statusType);
        }

        private void UpdateCounters()
        {
            _totalCycles++;

            if (_forwardSuccess && _reverseSuccess)
            {
                _successfulCycles++;
            }

            _forwardSuccess = false;
            _reverseSuccess = false;

            PublishCounterUpdate();
        }

        private bool ShouldStopTest()
        {
            if (_parameter.MaxCycles > 0 && _totalCycles >= _parameter.MaxCycles)
            {
                _isRunning = false;
                _machineMessage = $"已达到设定的最大循环次数 {_parameter.MaxCycles}，测试停止";
                PublishStateUpdate(MachineStatusType.Idle);
                return true;
            }

            int failedCycles = _totalCycles - _successfulCycles;
            if (_parameter.MaxFailures > 0 && failedCycles >= _parameter.MaxFailures)
            {
                _isRunning = false;
                _machineMessage = $"已达到设定的最大失败次数 {_parameter.MaxFailures}，测试停止";
                PublishStateUpdate(MachineStatusType.Error);
                return true;
            }

            return false;
        }

        private void PublishStateUpdate(MachineStatusType statusType)
        {
            if (_eventBus != null)
            {
                _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString(), statusType));
            }
        }

        private void PublishCounterUpdate()
        {
            if (_eventBus != null)
            {
                _eventBus.Publish(new CounterUpdateEvent(_totalCycles, _successfulCycles));
            }
        }

        private void SubscribeScriewMode()
        {
            try
            {
                device.ExecuteCommand(SasCommandType.Subscribe);
                _logger.Debug("已订阅101模式");
            }
            catch (Exception ex)
            {
                _logger.Error($"订阅101模式失败: {ex.Message}", ex);
            }
        }

        private async Task<(bool success, string errorMessage, int state, int result)> CheckLockStatusAsync()
        {
            if (_state == TestState.Reverse)
            {
                return (true, null, 0, 0);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                int retryCount = 0;

                while (stopwatch.ElapsedMilliseconds < _parameter.Timeout)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return (false, "操作已取消", -1, -1);
                    }

                    try
                    {
                        string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);

                        if (string.IsNullOrEmpty(jsonMsg))
                        {
                            _logger.Warn("设备返回空数据");
                            await Task.Delay(Constants.DEFAULT_POLLING_INTERVAL, _cancellationTokenSource.Token);
                            continue;
                        }

                        try
                        {
                            var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                            if (response != null && response.reply == 203 &&
                                response.state != null && response.result != null)
                            {
                                int state = (int)response.state;
                                int result = (int)response.result;

                                if (state == 0 && result == 0)
                                {
                                    return (true, null, state, result);
                                }
                                else
                                {
                                    string errorDesc = GetLockErrorDescription(result);
                                    return (false, errorDesc, state, result);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"JSON解析错误: {ex.Message}, 数据: {jsonMsg}");

                            if (++retryCount >= Constants.MAX_COMMUNICATION_RETRIES)
                            {
                                return (false, $"数据解析失败: {ex.Message}", -1, -1);
                            }

                            await Task.Delay(Constants.DEFAULT_RETRY_DELAY, _cancellationTokenSource.Token);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"通信错误: {ex.Message}");

                        if (++retryCount >= Constants.MAX_COMMUNICATION_RETRIES)
                        {
                            return (false, $"设备通信失败: {ex.Message}", -1, -1);
                        }

                        await Task.Delay(Constants.DEFAULT_RETRY_DELAY, _cancellationTokenSource.Token);
                    }

                    await Task.Delay(Constants.DEFAULT_POLLING_INTERVAL, _cancellationTokenSource.Token);
                }

                return (false, "正转锁付超时", -1, -1);
            }
            catch (OperationCanceledException)
            {
                return (false, "操作已取消", -1, -1);
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private string GetLockErrorDescription(int result)
        {
            if (result == 0) return "锁付成功";

            var errors = new List<string>();

            // 基础错误 (bit0-bit6)
            if ((result & (1 << 0)) != 0) errors.Add("电机堵转");
            if ((result & (1 << 1)) != 0) errors.Add("用户停止");
            if ((result & (1 << 2)) != 0) errors.Add("角度超限");
            if ((result & (1 << 3)) != 0) errors.Add("超时");
            if ((result & (1 << 4)) != 0) errors.Add("电机错误");
            if ((result & (1 << 5)) != 0) errors.Add("倾角超限");
            if ((result & (1 << 6)) != 0) errors.Add("扭力到达");

            // 参数超限错误 (bit17-bit29)
            if ((result & (1 << 17)) != 0) errors.Add("速度低于下限");
            if ((result & (1 << 18)) != 0) errors.Add("速度超过上限");
            if ((result & (1 << 19)) != 0) errors.Add("时间低于下限");
            if ((result & (1 << 20)) != 0) errors.Add("时间超过上限");
            if ((result & (1 << 21)) != 0) errors.Add("扭力低于下限");
            if ((result & (1 << 22)) != 0) errors.Add("扭力超过上限");
            if ((result & (1 << 23)) != 0) errors.Add("角度低于下限");
            if ((result & (1 << 24)) != 0) errors.Add("角度超过上限");
            if ((result & (1 << 25)) != 0) errors.Add("夹紧扭力低于下限");
            if ((result & (1 << 26)) != 0) errors.Add("夹紧扭力超过上限");
            if ((result & (1 << 27)) != 0) errors.Add("夹紧角度低于下限");
            if ((result & (1 << 28)) != 0) errors.Add("夹紧角度超过上限");
            if ((result & (1 << 29)) != 0) errors.Add("空转");

            return string.Join(", ", errors);
        }

        private static bool IsUserStoppedError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return false;

            return errorMessage.Contains("用户停止") || errorMessage.Contains("操作已取消");
        }


        //正转前的状态清理
        private async Task ClearDeviceStateBeforeForward()
        {
            try
            {
                _logger.Debug("正转前检查并清理设备状态");  

                // 检查设备当前状态
                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);

                if (!string.IsNullOrEmpty(jsonMsg))
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                        if (response != null && response.reply == 203 &&
                            response.state != null && response.result != null)
                        {
                            int state = (int)response.state;
                            int result = (int)response.result;

                            if (state != 0 || result != 0)
                            {
                                string errorDesc = GetLockErrorDescription(result);
                                _logger.Info($"正转前检测到设备错误状态，执行清理: state={state}, result={result}, 错误: {errorDesc}");

                                // 清理错误状态
                                device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                                await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME);

                                _logger.Debug("正转前设备状态清理完成");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"正转前检查设备状态JSON解析失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"正转前清理设备状态失败: {ex.Message}");
            }
        }

        //初始化时的设备状态验证
        private async Task InitializeAndVerifyDeviceState()
        {
            try
            {
                _logger.Info("初始化并验证设备状态");

                // 先订阅设备
                SubscribeScriewMode();

                // 等待订阅生效
                await Task.Delay(300);

                // 检查设备是否有残留的错误状态
                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);

                if (!string.IsNullOrEmpty(jsonMsg))
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                        if (response != null && response.reply == 203 &&
                            response.state != null && response.result != null)
                        {
                            int state = (int)response.state;
                            int result = (int)response.result;

                            if (state != 0 || result != 0)
                            {
                                string errorDesc = GetLockErrorDescription(result);
                                _logger.Warn($"检测到设备残留错误状态: state={state}, result={result}, 错误: {errorDesc}");

                                // 清理残留的错误状态
                                device.ExecuteCommand(SasCommandType.Stop);
                                await Task.Delay(200);

                                device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                                await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME);

                                device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                                await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME);

                                // 重新订阅
                                device.ExecuteCommand(SasCommandType.Subscribe);
                                await Task.Delay(300);

                                _logger.Info("已清理设备残留错误状态，重新初始化完成");
                            }
                            else
                            {
                                _logger.Info("设备状态正常，初始化完成");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"初始化时检查设备状态JSON解析失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"初始化设备状态失败: {ex.Message}");
            }
        }

        //设备状态验证
        private async Task VerifyDeviceStateCleared()
        {
            try
            {
                _logger.Info("验证设备状态是否已清理干净");

                // 等待一段时间让设备状态稳定
                await Task.Delay(300);

                // 检查设备状态
                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);

                if (!string.IsNullOrEmpty(jsonMsg))
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                        if (response != null && response.reply == 203 &&
                            response.state != null && response.result != null)
                        {
                            int state = (int)response.state;
                            int result = (int)response.result;

                            if (state != 0 || result != 0)
                            {
                                _logger.Warn($"设备状态未完全清理: state={state}, result={result}");

                                // 再次尝试清理
                                device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                                await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME);

                                _logger.Info("已执行额外的设备状态清理");
                            }
                            else
                            {
                                _logger.Info("设备状态验证通过，状态已清理干净");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"验证设备状态时JSON解析失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"验证设备状态失败: {ex.Message}");
            }
        }
        #endregion
    }
}




