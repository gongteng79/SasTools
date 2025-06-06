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

        public TestStateMachine(IDevice device, FatigueParams parameter, IEventBus eventBus)
        {
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter), "参数服务不能为空");
            this.device = device;
            _eventBus = eventBus;
        }

        #region 公共方法
        public bool StartTest()
        {
            _isRunning = true;
            _state = TestState.Idle;
            _cancellationTokenSource = new CancellationTokenSource();
            testMachineThread = new Thread(() => RunStateMachineAsync(_cancellationTokenSource.Token));
            testMachineThread.Start();

            return true;
        }

        public bool StopTest()
        {
            _isRunning = false;
            _state = TestState.Idle;
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            device.ExecuteCommand(SasCommandType.Stop);
            testMachineThread = null;
            return true;
        }

        #endregion

        private async void RunStateMachineAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_state == null)
                    {
                        continue;
                    }
                    if (!_isRunning)
                    {
                        return;
                    }

                    await TestLockingScrewsAsync();
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
                _state = TestState.Error;
            }
        }

        private async Task TestLockingScrewsAsync()
        {
            switch (_state)
            {
                case TestState.Idle:
                    _state = TestState.Initializing;
                    break;

                case TestState.Initializing:
                    SubscribeScriewMode();
                    _state = TestState.ForwardDelay; 
                    _machineMessage = "初始化完成..";
                    break;

                case TestState.ForwardDelay:
                    await Task.Delay(_parameter.ForwardDelay);
                    _state = TestState.Forward;
                    _machineMessage = "已启动正转延时..";
                    break;

                case TestState.Forward:
                    var forwardResult = device.ExecuteCommand(SasCommandType.Forward);
                    await CheckLockStatusAsync();
                    _state = TestState.ReverseDelay;
                    _machineMessage = "正转OK..";
                    break;

                case TestState.ReverseDelay:
                    await Task.Delay(_parameter.ReverseDelay);
                    _state = TestState.Reverse;
                    _machineMessage = "已启动反转延时..";
                    break;

                case TestState.Reverse:
                    var reverseResult = device.ExecuteCommand(SasCommandType.Reverse);
                    await CheckLockStatusAsync();
                    _state = TestState.Stopping;
                    _machineMessage = "反转OK..";
                    break;

                case TestState.Stopping:
                    device.ExecuteCommand(SasCommandType.Stop);
                    _state = TestState.ForwardDelay;
                    _machineMessage = "运行结束..";
                    break;

                case TestState.Error:
                    _machineMessage = "状态机进入错误状态";
                    break;
            }

            _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString()));
        }


        private void SubscribeScriewMode()
        {
            device.ExecuteCommand(SasCommandType.Subscribe);
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

        // 异步检查锁付状态函数
        private async Task<bool> CheckLockStatusAsync()
        {
            bool isCompleted = false;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int retryCount = 0;
            const int MaxRetries = 3;

            while (!isCompleted && stopwatch.ElapsedMilliseconds < _parameter.Timeout)
            {
                try
                {
                    string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);

                    // 记录原始响应便于调试
                    _logger.Debug($"收到设备响应: {jsonMsg}");

                    try
                    {
                        var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                        if (response != null && response.reply == 203)
                        {
                            // 检查state和result是否存在且不为null
                            if (response.state != null && response.result != null)
                            {
                                int state = (int)response.state;
                                int result = (int)response.result;

                                // 检查正转状态下的成功条件
                                if (_state == TestState.Forward)
                                {
                                    if (state == 0 && result == 0)
                                    {
                                        // 正转操作成功
                                        isCompleted = true;
                                    }
                                    else if (state != 0 || result != 0)
                                    {
                                        // 正转操作失败
                                        string errorDesc = GetLockErrorDescription(result);
                                        _logger.Error($"正转锁付失败: state={state}, result={result}, 错误: {errorDesc}");
                                        _state = TestState.Error;
                                        _machineMessage = $"正转失败: {errorDesc}";
                                        _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString()));
                                        device.ExecuteCommand(SasCommandType.Stop);
                                        return false;
                                    }
                                }
                                // 检查反转状态下的成功条件
                                else if (_state == TestState.Reverse)
                                {
                                    if (state == 0)
                                    {
                                        // 反转操作成功
                                        isCompleted = true;
                                    }
                                    else
                                    {
                                        // 反转操作失败
                                        string errorDesc = GetLockErrorDescription(result);
                                        _logger.Error($"反转锁付失败: state={state}, result={result}, 错误: {errorDesc}");
                                        _state = TestState.Error;
                                        _machineMessage = $"反转失败: {errorDesc}";
                                        _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString()));
                                        device.ExecuteCommand(SasCommandType.Stop);
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                // state或result字段为null，记录日志并继续轮询
                                _logger.Warn($"设备响应字段不完整: state或result为null，继续等待完整数据");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // JSON解析异常处理
                        _logger.Error($"处理消息失败: {ex.Message}", ex);
                        retryCount++;

                        if (retryCount >= MaxRetries)
                        {
                            _state = TestState.Error;
                            _machineMessage = $"数据解析失败: {ex.Message}";
                            _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString()));
                            device.ExecuteCommand(SasCommandType.Stop);
                            return false;
                        }

                        // 短暂延迟后重试
                        await Task.Delay(100);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    // 通信异常处理
                    _logger.Error($"设备通信错误: {ex.Message}", ex);
                    retryCount++;

                    if (retryCount >= MaxRetries)
                    {
                        _state = TestState.Error;
                        _machineMessage = $"设备通信失败: {ex.Message}";
                        _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString()));
                        device.ExecuteCommand(SasCommandType.Stop);
                        return false;
                    }

                    // 短暂延迟后重试
                    await Task.Delay(100);
                }

                // 添加短暂延迟，避免CPU高占用
                await Task.Delay(50);
            }

            // 检查是否超时
            if (!isCompleted)
            {
                string operation = _state == TestState.Forward ? "正转" : "反转";
                _logger.Error($"{operation}锁付操作超时: {_parameter.Timeout}ms内未完成");
                _state = TestState.Error;
                _machineMessage = $"{operation}锁付超时";
                _eventBus.Publish(new RefreshMachineState(_machineMessage, _state.ToString()));
                device.ExecuteCommand(SasCommandType.Stop);
                return false;
            }

            return true;
        }


        // 同步版本函数
        private bool CheckLockStatus()
        {
            return CheckLockStatusAsync().GetAwaiter().GetResult();
        }
    }
}



