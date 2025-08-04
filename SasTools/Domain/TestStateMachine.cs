using AntdUI;
using log4net;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Events;
using SasTools.Infrastructure;
using SasTools.Interface;
using SasTools.Models;
using SasTools.Models.Protocol;
using SasTools.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Domain
{
    // 测试状态机实现类
    public class TestStateMachine:IDisposable
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
        private IDevice device;
        private readonly string _deviceId;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _resetCancellationTokenSource;
        private bool _isRunning = false;
        private bool _isPause = false;
        private DeviceResponseParser _responseParser;
            
        // 状态相关
        private TestState _state = TestState.Idle;
        private string _machineMessage = "";

        // 计数器相关
        private int _totalCycles = 0;
        private int _successfulCycles = 0;
        private int _failedCycles = 0;
        private bool _forwardSuccess = false;
        private bool _reverseSuccess = false;

        // 错误处理相关
        private string _lastErrorMessage = "";
        private int _lastErrorResult = 0;

        // 事件节流控制
        private DateTime _lastStatePublishTime = DateTime.MinValue;
        private DateTime _lastCounterPublishTime = DateTime.MinValue;
        private readonly TimeSpan _statePublishInterval = TimeSpan.FromMilliseconds(100); // 状态更新间隔
        private readonly TimeSpan _counterPublishInterval = TimeSpan.FromMilliseconds(200); // 计数器更新间隔
                                                                                            // 状态变化跟踪
        private MachineStatusType _lastPublishedStatusType = MachineStatusType.Idle;
        private string _lastPublishedMessage = string.Empty;
        private readonly DeviceStateManager _deviceStateManager;
        // 定义关键状态，这些状态变化需要立即发布
        private readonly HashSet<MachineStatusType> _criticalStatusTypes = new HashSet<MachineStatusType>
        {
            MachineStatusType.Error,
            MachineStatusType.Idle,
            MachineStatusType.Forward,
            MachineStatusType.Reverse
        };


        #endregion

        #region 构造函数
        public TestStateMachine(string deviceId, IDevice device, FatigueParams parameter, IEventBus eventBus)
        {
            _deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            //初始化解析器
            _responseParser = new DeviceResponseParser(_logger);

            // 初始化设备状态管理器
            _deviceStateManager = new DeviceStateManager(_logger);

            // 初始化事件订阅管理器
            _subscriptionManager = new EventSubscriptionManager();
            // 修改为无参构造
            _subscriptionManager = new EventSubscriptionManager();
            // 初始化取消令牌源
            _cancellationTokenSource = new CancellationTokenSource();
            _resetCancellationTokenSource = new CancellationTokenSource();

            _logger.Info($"状态机初始化完成 - 设备: {_deviceId}");
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
            }, _cancellationTokenSource.Token);

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

            // 异步执行停止命令，避免阻塞UI线程
            Task.Run(async () =>
            {
                try
                {
                    await device.ExecuteCommandAsync(SasCommandType.Stop);
                    _logger.Info("测试已停止");
                }
                catch (Exception ex)
                {
                    _logger.Error($"停止设备时出错: {ex.Message}", ex);
                }
            });

            return true;
        }

        public void ResetCounters()
        {
            _totalCycles = 0;
            _successfulCycles = 0;
            _failedCycles = 0;
            _forwardSuccess = false;
            _reverseSuccess = false;
            _lastErrorMessage = "";
            _lastErrorResult = 0;
            PublishCounterUpdate();
            _logger.Info("计数器已重置");
        }

        // 简化后的Reset方法，使用统一的设备状态管理器
        public void Reset()
        {
            try
            {
                _logger.Info($"开始重置状态机 - 设备: {_deviceId}");

                // 停止当前运行
                _isRunning = false;
                _isPause = false;

                // 取消当前操作
                _cancellationTokenSource?.Cancel();

                // 重置状态
                _state = TestState.Idle;
                _totalCycles = 0;
                _successfulCycles = 0;
                _failedCycles = 0;
                _forwardSuccess = false;
                _reverseSuccess = false;
                _machineMessage = "已重置";

                // 使用统一的设备状态管理器进行清理
                _ = Task.Run(async () =>
                {
                    await _deviceStateManager.ResetDeviceStateAsync(device, _resetCancellationTokenSource.Token);
                }, _resetCancellationTokenSource.Token);

                // 发布重置状态
                PublishStateUpdate(MachineStatusType.Idle);

                _logger.Info($"状态机重置完成 - 设备: {_deviceId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"重置状态机失败 - 设备: {_deviceId}, 错误: {ex.Message}", ex);
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
                _eventBus.Publish(new RefreshMachineState(_deviceId, "执行出错: " + ex.Message, _state.ToString(), MachineStatusType.Error));
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
                        await _deviceStateManager.CheckAndCleanDeviceStateAsync(device, _cancellationTokenSource.Token, includeSubscribe: true);
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
                        await _deviceStateManager.CheckAndCleanDeviceStateAsync(device, _cancellationTokenSource.Token);

                        _logger.Info("发送正转命令到设备");
                        var forwardResponse = device.ExecuteCommand(SasCommandType.Forward);

                        _logger.Info($"正转命令执行结果: {forwardResponse}");

                        await Task.Delay(500, _cancellationTokenSource.Token);
                        var result = await CheckLockStatusAsync();
                        _logger.Info($"正转状态检查结果: success={result.success}, state={result.state}, result={result.result}, error={result.errorMessage}");

                        if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (result.success)
                        {
                            _logger.Info("正转锁付成功");
                            _forwardSuccess = true;

                            // 等待正转操作完全结束的延时
                            await Task.Delay(1000, _cancellationTokenSource.Token);
                            _machineMessage = "正转OK";
                            _lastPublishedMessage = "";
                            // 发布正转OK状态
                            PublishStateUpdate(MachineStatusType.Forward);
                            _logger.Info("已发布正转OK状态更新");

                            // 然后转换到下一个状态
                            _state = TestState.ReverseDelay;
                            shouldUpdateDisplay = false;
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
                            _forwardSuccess = false;
                            _state = TestState.Error;
                            _machineMessage = "正转NG";
                            PublishStateUpdate(MachineStatusType.Error);
                            device.ExecuteCommand(SasCommandType.Stop);
                            UpdateCounters();
                            if (ShouldStopTest())
                            {
                                _isRunning = false;
                            }
                            shouldUpdateDisplay = false;
                        }
                        break;

                    case TestState.ReverseDelay:
                        _machineMessage = "反转延时中...";
                        PublishStateUpdate(MachineStatusType.Waiting);
                        shouldUpdateDisplay = false; 

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

                    // 第375-399行的Reverse状态处理
                    case TestState.Reverse:
                        _machineMessage = "执行反转...";
                        PublishStateUpdate(MachineStatusType.Reverse);
                        shouldUpdateDisplay = false;

                        _logger.Info($"执行反转命令 - 速度: {_parameter.ReverseVelocity}, 时间: {_parameter.ReverseTime}");

                        // 创建包含完整参数的CommandParameters
                        var commandParams = new CommandParameters
                        {
                            Velocity = _parameter.ReverseVelocity,
                            Time = _parameter.ReverseTime,
                        };

                        // 使用协议处理器直接执行命令
                        if (device.HasProtocolHandler())
                        {
                            var protocolHandler = device.GetProtocolHandler();
                            var reverseResponse = await protocolHandler.ExecuteCommandAsync(SasCommandType.Reverse, commandParams);
                            _logger.Info($"反转命令执行结果: {reverseResponse.Success}, 消息: {reverseResponse.Message}");
                        }
                        else
                        {
                            var reverseResult = await device.ExecuteCommandWithParametersAsync(SasCommandType.Reverse, _parameter.ReverseVelocity, _parameter.ReverseTime);
                            _logger.Info($"反转命令执行结果: {reverseResult}");
                        }

                        try
                        {
                            int waitTime = _parameter.ReverseTime + 500;
                            _logger.Info($"等待反转完成，等待时间: {waitTime}ms");
                            await Task.Delay(waitTime, _cancellationTokenSource.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }

                        if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        // 验证反转是否真正执行
                        _logger.Info("验证反转执行状态");

                        _reverseSuccess = true;
                        _state = TestState.Stopping;
                        _machineMessage = "反转OK";
                        PublishStateUpdate(MachineStatusType.Reverse);
                        _logger.Info("反转状态完成");
                        shouldUpdateDisplay = false;
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
            try
            {
                // 检查是否应该停止测试
                if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _logger.Info("测试已停止，退出错误处理");
                    return;
                }

                var strategy = DetermineRecoveryStrategy(_lastErrorResult, _lastErrorMessage);

                _logger.Info($"错误类型: {_lastErrorMessage}, 采用恢复策略: {strategy.Name}");

                // 不要重复发布Error状态，因为在Forward状态中已经发布过了
                _machineMessage = "错误状态，分析处理中";
                PublishStateUpdate(MachineStatusType.Error);

                await ExecuteRecoveryStrategyAsync(strategy);

                // 检查恢复后是否应该继续
                if (!_isRunning || _cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _logger.Info("恢复策略执行后检测到停止请求");
                    return;
                }

                // 恢复成功，重新开始循环
                _state = TestState.ForwardDelay;
                _machineMessage = "错误恢复完成，准备重试";
                PublishStateUpdate(MachineStatusType.Waiting);
            }
            catch (OperationCanceledException)
            {
                _logger.Info("错误处理被取消");
                // 不重新抛出，让状态机正常退出
            }
            catch (Exception ex)
            {
                _logger.Error($"错误处理失败: {ex.Message}", ex);
                _state = TestState.Error;
                throw;
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

                // 检查取消状态
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _logger.Info("恢复策略执行前检测到取消请求");
                    return;
                }

                await Task.Delay(strategy.WaitTime, _cancellationTokenSource.Token);

                if (strategy.RequiresClearTightenInfo)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                    device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                    _logger.Info("已清理设备错误状态");
                    await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME, _cancellationTokenSource.Token);
                }

                if (strategy.RequiresResubscribe)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                    device.ExecuteCommand(SasCommandType.Subscribe);
                    _logger.Info("已重新订阅设备");
                    await Task.Delay(300, _cancellationTokenSource.Token);
                }

                if (strategy.RequiresExtraDelay)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                    _machineMessage = "额外稳定延时中...";
                    await Task.Delay(RecoveryTimes.EXTRA_STABILITY_DELAY, _cancellationTokenSource.Token);
                }

                _logger.Info($"恢复策略 {strategy.Name} 执行完成");
            }
            catch (OperationCanceledException)
            {
                _logger.Info("恢复策略执行被取消");
            }
            catch (Exception ex)
            {
                _logger.Error($"执行恢复策略失败: {ex.Message}", ex);
                throw;
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
            else
            {
                _failedCycles++;
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
            try
            {
                if (_eventBus != null && !_disposed)
                {
                    var now = DateTime.Now;
                    bool isCritical = _criticalStatusTypes.Contains(statusType);

                    // 检查状态是否真正发生变化
                    bool statusChanged = statusType != _lastPublishedStatusType ||_machineMessage != _lastPublishedMessage;

                    _logger.Debug($"状态发布检查 - 类型:{statusType}, 消息:{_machineMessage}, 状态变化:{statusChanged}, 关键状态:{isCritical}");

                    // 如果状态未变化且非关键状态，跳过发布
                    if (!statusChanged && !isCritical)
                    {
                        return;
                    }

                    // 非关键状态进行节流检查
                    if (!isCritical && now - _lastStatePublishTime < _statePublishInterval)
                    {
                        return;
                    }

                    // 更新时间戳和状态跟踪
                    _lastStatePublishTime = now;
                    _lastPublishedStatusType = statusType;
                    _lastPublishedMessage = _machineMessage;

                    _eventBus.Publish(new RefreshMachineState(_deviceId, _machineMessage, _state.ToString(), statusType));
                }
            }
            catch (Exception ex)
            {
                _logger?.Warn($"发布状态更新事件失败: {ex.Message}");
            }
        }
        private void PublishCounterUpdate()
        {
            try
            {
                if (_eventBus != null && !_disposed)
                {
                    var now = DateTime.Now;

                    if (now - _lastCounterPublishTime < _counterPublishInterval)
                    {
                        return;
                    }
                    _lastCounterPublishTime = now;
                    _eventBus.Publish(new CounterUpdateEvent(_deviceId, _totalCycles, _successfulCycles, _failedCycles));
                }
            }
            catch (Exception ex)
            {
                _logger?.Warn($"发布计数器更新事件失败:{ex.Message}");
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
                        var parseResult = _responseParser.ParseResponse(jsonMsg);

                        _logger.Debug($"JSON原始数据: {jsonMsg}");
                        _logger.Debug($"解析结果: Success={parseResult.Success}, State={parseResult.State}, Result={parseResult.Result}");

                        if (parseResult.Success)
                        {
                            if (_responseParser.IsOperationCompleted(parseResult))
                            {
                                if (_responseParser.IsCompletedOperationSuccessful(parseResult))
                                {
                                    _logger.Info($"锁付操作成功: state={parseResult.State}, result={parseResult.Result}");
                                    return (true, null, parseResult.State, parseResult.Result);
                                }
                                else
                                {
                                    string errorDesc = _responseParser.GetLockErrorDescription(parseResult.Result);
                                    _logger.Warn($"锁付操作失败: state={parseResult.State}, result={parseResult.Result}, 错误: {errorDesc}");
                                    return (false, errorDesc, parseResult.State, parseResult.Result);
                                }
                            }
                            else if (_responseParser.IsDeviceWorking(parseResult))
                            {
                                _logger.Debug($"设备正在执行锁付: state={parseResult.State}，继续等待");
                                await Task.Delay(Constants.DEFAULT_POLLING_INTERVAL, _cancellationTokenSource.Token);
                                continue;
                            }
                            else
                            {
                                _logger.Warn($"设备状态未知: state={parseResult.State}");
                                return (false, $"设备状态未知: {parseResult.State}", parseResult.State, parseResult.Result);
                            }
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

        private static bool IsUserStoppedError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return false;

            return errorMessage.Contains("用户停止") || errorMessage.Contains("操作已取消");
        }
        #endregion

        #region IDisposable实现
        public void Dispose()
        {
            if (!_disposed)
            {
                _logger.Info($"开始释放状态机资源 - 设备: {_deviceId}");

                // 1. 停止运行状态
                _isRunning = false;

                // 2. 取消所有异步操作
                _cancellationTokenSource?.Cancel();
                _resetCancellationTokenSource?.Cancel();

                // 3. 取消事件订阅
                _subscriptionManager?.UnsubscribeAll(_deviceId);

                // 4. 释放CancellationTokenSource
                _cancellationTokenSource?.Dispose();
                _resetCancellationTokenSource?.Dispose();

                // 5. 释放订阅管理器
                _subscriptionManager?.Dispose();

                // 6. 清理设备引用
                device = null;

                _disposed = true;
                _logger.Info($"状态机资源释放完成 - 设备: {_deviceId}");
            }
        }

        private bool _disposed = false;
        private readonly EventSubscriptionManager _subscriptionManager;
        #endregion
    }
}




