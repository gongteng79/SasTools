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


namespace SasTools.Domain
{
    // 测试状态机实现类
    public class TestStateMachine
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TestStateMachine));
        private readonly IParameterService _parameterService;
        private IEventBus _eventBus;
        private TestState _state = TestState.Idle;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private bool _isPause = false;
        private Thread testMachineThread;
        private IDevice device;

        public TestStateMachine(IDevice device, IParameterService parameterService, IEventBus eventBus)
        {
            this.device = device;
            //_parameterService = parameterService ?? throw new ArgumentNullException(nameof(parameterService));
            _eventBus = eventBus;
        }

        #region 公共方法
        public bool StartTest()
        {
            _isRunning = true;
            _state = TestState.Idle;
            _cancellationTokenSource = new CancellationTokenSource();
            testMachineThread = new Thread(() => RunStateMachineAsync(_cancellationTokenSource.Token));

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
                    break;

                case TestState.ForwardDelay:
                    await Task.Delay(2000);
                    _state = TestState.Forward;
                    break;

                case TestState.Forward:
                    var forwardResult = device.ExecuteCommand(SasCommandType.Forward);
                    CheckLockStatusAsync();
                    _state = TestState.InputScrewData;
                    break;

                case TestState.InputScrewData:
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
                    var reverseResult = device.ExecuteCommand(SasCommandType.Reverse);
                    CheckLockStatusAsync();
                    _state = TestState.ReverseWaiting;
                    break;

                case TestState.ReverseWaiting:
                    await Task.Delay(1000);
                    _state = TestState.StartupInterval;
                    break;

                case TestState.StartupInterval:
                    await Task.Delay(1000);
                    _state = TestState.Stopping;
                    break;

                case TestState.Stopping:
                    device.ExecuteCommand(SasCommandType.Stop);
                    _state = TestState.ForwardDelay;
                    break;

                case TestState.Error:
                    _logger.Error("状态机进入错误状态");
                    break;
            }

            _eventBus.Publish(new SendDataEvent(string.Empty, _state.ToString()));
        }


        private void SubscribeScriewMode()
        {
            device.ExecuteCommand(SasCommandType.Subscribe);
        }

        private void CheckLockStatusAsync()
        {
            while (true)
            {
                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                try
                {
                    var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                    if (response.reply == 203)
                    {
                        int state = (int)response.state;
                        if ((_state == TestState.Forward && state == 0 && response.result == 0) || (_state == TestState.Reverse && state == 0))
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"处理消息失败: {ex.Message}", ex);
                    break;
                }
            }
        }
    }
}



