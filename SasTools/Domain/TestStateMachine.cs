using log4net;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Interface;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SasTools.Services;
using System.Text.Json;
using static System.Windows.Forms.AxHost;
using WpFramework.EventBus;
using SasTools.Events;
using System.Runtime.InteropServices;
using System.Collections.Generic;


namespace SasTools.Domain
{
    // 测试状态机实现类
    public class TestStateMachine
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TestStateMachine));//日志记录器
        private readonly FatigueParams _parameter;//测试参数
        private IEventBus _eventBus;//事件总线，用于发布状态更新事件        
        private string _machineMessage;//状态机消息
        private TestState _state = TestState.Idle;//当前状态
        private CancellationTokenSource _cancellationTokenSource;//取消令牌源，用于控制状态机的运行
        private bool _isRunning = false;//运行标志
        private bool _isPause = false;//暂停标志
        private Thread testMachineThread;//测试线程
        private IDevice device;//设备接口
        private int _totalCycles = 0; // 总循环次数
        private int _successfulCycles = 0; // 成功循环次数
        private bool _forwardSuccess = false; // 正转成功标志
        private bool _reverseSuccess = false; // 反转成功标志

        public TestStateMachine(IDevice device, FatigueParams parameter, IEventBus eventBus)
        {
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter), "参数服务不能为空");
            this.device = device ?? throw new ArgumentNullException(nameof(device), "设备接口不能为空");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus), "事件总线不能为空");
        }

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

        // 重置计数器方法
        public void ResetCounters()
        {
            _totalCycles = 0;
            _successfulCycles = 0;
            _forwardSuccess = false;
            _reverseSuccess = false;
            // 发布计数器更新事件
            PublishCounterUpdate();
            _logger.Info("计数器已重置");
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
                        await Task.Delay(100, cancellationToken);
                        continue;
                    }

                    await TestLockingScrewsAsync();

                    // 添加小延迟避免CPU占用过高
                    await Task.Delay(10, cancellationToken);
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
                switch (_state)
                {
                    case TestState.Idle:
                        _state = TestState.Initializing;
                        break;

                    case TestState.Initializing:
                        SubscribeScriewMode();
                        _state = TestState.ForwardDelay;
                        _machineMessage = "初始化完成";
                        break;

                    case TestState.ForwardDelay:
                        _machineMessage = "正转延时中...";
                        PublishStateUpdate(MachineStatusType.Waiting);
                        await Task.Delay(_parameter.ForwardDelay);
                        _state = TestState.Forward;
                        break;

                    case TestState.Forward:
                        _machineMessage = "执行正转...";
                        PublishStateUpdate(MachineStatusType.Forward);

                        device.ExecuteCommand(SasCommandType.Forward);
                        var (success, errorMessage, state, result) = await CheckLockStatusAsync();

                        if (success)
                        {
                            _logger.Info("正转锁付成功");
                            _forwardSuccess = true;
                            _state = TestState.ReverseDelay;
                            _machineMessage = "正转OK";
                            PublishStateUpdate(MachineStatusType.Forward);
                        }
                        else
                        {
                            _logger.Error($"正转锁付失败: state={state}, result={result}, 错误: {errorMessage}");
                            _state = TestState.Error;
                            _machineMessage = $"正转失败: {errorMessage}";
                            PublishStateUpdate(MachineStatusType.Error);
                            device.ExecuteCommand(SasCommandType.Stop);
                        }
                        break;

                    case TestState.ReverseDelay:
                        _machineMessage = "反转延时中...";
                        PublishStateUpdate(MachineStatusType.Waiting);
                        await Task.Delay(_parameter.ReverseDelay);
                        _state = TestState.Reverse;
                        break;

                    case TestState.Reverse:
                        _machineMessage = "执行反转...";
                        PublishStateUpdate(MachineStatusType.Reverse);

                        // 通过设置Reverse.Time值控制反转，等待时间完成后直接认为反转成功
                        device.ExecuteCommand(SasCommandType.Reverse);
                        await Task.Delay(2000);
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
                        _machineMessage = "错误状态，等待恢复";
                        PublishStateUpdate(MachineStatusType.Error);
                        await Task.Delay(5000);
                        _state = TestState.Idle;
                        break;

                    default:
                        _logger.Warn($"未处理的状态: {_state}");
                        _state = TestState.Idle;
                        break;
                }

                UpdateStatusDisplay();
            }
            catch (Exception ex)
            {
                _logger.Error($"状态处理错误: {ex.Message}", ex);
                _state = TestState.Error;
                _machineMessage = $"处理错误: {ex.Message}";
                PublishStateUpdate(MachineStatusType.Error);
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
                case TestState.ReverseDelay:
                case TestState.StartupInterval:
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
            // 增加总循环次数
            _totalCycles++;

            // 如果正转和反转都成功，则增加成功循环次数
            if (_forwardSuccess && _reverseSuccess)
            {
                _successfulCycles++;
            }

            // 重置单次测试状态
            _forwardSuccess = false;
            _reverseSuccess = false;

            // 发布计数器更新事件
            PublishCounterUpdate();
        }

        private bool ShouldStopTest()
        {
            // 检查是否达到最大循环次数
            if (_parameter.MaxCycles > 0 && _totalCycles >= _parameter.MaxCycles)
            {
                _isRunning = false;
                _machineMessage = $"已达到设定的最大循环次数 {_parameter.MaxCycles}，测试停止";
                PublishStateUpdate(MachineStatusType.Idle);
                return true;
            }

            // 计算失败次数
            int failedCycles = _totalCycles - _successfulCycles;

            // 检查是否达到最大失败次数
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

        public bool PauseTest()
        {
            if (!_isRunning || _isPause) return false;

            _isPause = true;
            _machineMessage = "测试已暂停";
            PublishStateUpdate(MachineStatusType.Waiting);
            _logger.Info("测试已暂停");
            return true;
        }

        public bool ResumeTest()
        {
            if (!_isRunning || !_isPause) return false;

            _isPause = false;
            _machineMessage = "测试已恢复";
            PublishStateUpdate(MachineStatusType.Normal);
            _logger.Info("测试已恢复");
            return true;
        }

        private async Task<(bool success, string errorMessage, int state, int result)> CheckLockStatusAsync()
        {
            // 如果是反转状态，直接返回成功
            if (_state == TestState.Reverse)
            {
                return (true, null, 0, 0);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                int retryCount = 0;
                const int MaxRetries = 3;
                const int RetryDelay = 100;
                const int PollingInterval = 50;

                while (stopwatch.ElapsedMilliseconds < _parameter.Timeout)
                {
                    // 检查取消令牌
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
                            await Task.Delay(PollingInterval, _cancellationTokenSource.Token);
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

                                // 检查成功条件
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

                            if (++retryCount >= MaxRetries)
                            {
                                return (false, $"数据解析失败: {ex.Message}", -1, -1);
                            }

                            await Task.Delay(RetryDelay, _cancellationTokenSource.Token);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"通信错误: {ex.Message}");

                        if (++retryCount >= MaxRetries)
                        {
                            return (false, $"设备通信失败: {ex.Message}", -1, -1);
                        }

                        await Task.Delay(RetryDelay, _cancellationTokenSource.Token);
                    }

                    await Task.Delay(PollingInterval, _cancellationTokenSource.Token);
                }

                return (false, "正转锁付超时", -1, -1);
            }
            catch (OperationCanceledException)
            {
                return (false, "操作已取消", -1, -1);
            }
            finally
            {
                // 停止计时器
                stopwatch.Stop();
            }
        }

        // 错误解析辅助方法
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
        #endregion
    }
}



