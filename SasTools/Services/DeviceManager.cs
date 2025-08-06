using log4net;
using SasTools.Domain;
using SasTools.Events;
using SasTools.Interface;
using SasTools.Models;
using SasTools.Models.SasModule;
using SasTools.Models.Protocol;
using SasTools.Services;
using SasTools.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Services
{
    public class DeviceManager
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(DeviceManager));
        private readonly IEventBus _eventBus;
        private readonly ConcurrentDictionary<string, DeviceInfo> _deviceInfos;
        private readonly ConcurrentDictionary<string, IDevice> _devices;
        private readonly ConcurrentDictionary<string, ICommunicationService> _communicationServices;
        private readonly ConcurrentDictionary<string, IProtocolHandler> _protocolHandlers; //协议处理器管理
        private readonly List<string> _deviceOrder; // 跟踪设备添加顺序
        private readonly object _orderLock = new object(); // 保护顺序列表的锁
        private string _selectedDeviceId;
        private readonly Dictionary<string, bool> _deviceTestStates = new Dictionary<string, bool>();
        private readonly Dictionary<string, TestStateMachine> _deviceStateMachines = new Dictionary<string, TestStateMachine>();

        public DeviceManager(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _deviceInfos = new ConcurrentDictionary<string, DeviceInfo>();
            _devices = new ConcurrentDictionary<string, IDevice>();
            //可供外部订阅的事件
            _communicationServices = new ConcurrentDictionary<string, ICommunicationService>();
            _protocolHandlers = new ConcurrentDictionary<string, IProtocolHandler>();
            _deviceOrder = new List<string>();
        }


        public event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;
        public event EventHandler<string> DeviceSelectionChanged;

        public IEnumerable<DeviceInfo> GetAllDevices()
        {
            lock (_orderLock)
            {
                var orderedDevices = new List<DeviceInfo>();
                foreach (var deviceId in _deviceOrder)
                {
                    if (_deviceInfos.TryGetValue(deviceId, out var deviceInfo))
                    {
                        orderedDevices.Add(deviceInfo);
                    }
                }
                return orderedDevices;
            }
        }

        public DeviceInfo GetDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            _deviceInfos.TryGetValue(deviceId, out var deviceInfo);
            return deviceInfo;
        }

        public IDevice GetDeviceInstance(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            _devices.TryGetValue(deviceId, out var device);
            return device;
        }

        //获取设备协议处理器
        public IProtocolHandler GetDeviceProtocolHandler(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            _protocolHandlers.TryGetValue(deviceId, out var handler);
            return handler;
        }

        public bool AddDevice(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null || string.IsNullOrEmpty(deviceInfo.Id))
                return false;

            if (_deviceInfos.ContainsKey(deviceInfo.Id))
                return false;

            bool added = _deviceInfos.TryAdd(deviceInfo.Id, deviceInfo);
            if (added)
            {
                lock (_orderLock)
                {
                    _deviceOrder.Add(deviceInfo.Id);
                }
            }
            return added;
        }

        public async Task<bool> RemoveDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return false;

            // 先断开连接
            await DisconnectDevice(deviceId);

            // 移除设备信息
            _deviceInfos.TryRemove(deviceId, out _);
            _devices.TryRemove(deviceId, out _);
            _communicationServices.TryRemove(deviceId, out _);

            // 新增：释放协议处理器
            if (_protocolHandlers.TryRemove(deviceId, out var handler))
            {
                handler?.Dispose();
            }

            // 从顺序列表中移除
            lock (_orderLock)
            {
                _deviceOrder.Remove(deviceId);
            }
            // 发布设备移除事件
            _eventBus.Publish(new MultiDeviceRemoveEvent(deviceId));
            return true;
        }

        //支持协议选择的连接方法
        public async Task<bool> ConnectDevice(string deviceId)
        {
            try
            {
                if (!_deviceInfos.TryGetValue(deviceId, out var deviceInfo))
                {
                    _logger.Error($"设备不存在: {deviceId}");
                    return false;
                }

                if (deviceInfo.IsConnected)
                {
                    _logger.Warn($"设备已连接: {deviceInfo.Name}");
                    return true;
                }

                // 根据协议类型创建处理器或通信服务
                bool connected = false;
                IDevice device = null;

                if (deviceInfo.ProtocolConfig != null)
                {
                    // 使用新的协议处理器
                    var protocolHandler = ProtocolFactory.CreateProtocolHandler(deviceInfo.ProtocolType);

                    // 连接设备
                    connected = await protocolHandler.ConnectAsync(deviceInfo.ProtocolConfig);

                    if (connected)
                    {
                        // 创建设备实例
                        device = new SasDevice(protocolHandler, _eventBus);

                        // 保存协议处理器
                        _protocolHandlers.TryAdd(deviceId, protocolHandler);

                        _logger.Info($"设备连接成功 ({deviceInfo.ProtocolType}): {deviceInfo.Name} ({deviceInfo.ProtocolConfig.ConnectionString})");
                    }
                }
                else
                {
                    var communicationService = new CommunicationService(deviceInfo.Host, deviceInfo.Port);
                    communicationService.ConnectionStatusChanged += (sender, isConnected) =>
                    {
                        OnDeviceConnectionStatusChanged(deviceId, isConnected);
                    };

                    // 尝试真实连接
                    connected = await communicationService.ConnectAsync();

                    if (connected)
                    {
                        // 创建设备实例
                        device = new SasDevice(communicationService, _eventBus);

                        // 保存通信服务
                        _communicationServices.TryAdd(deviceId, communicationService);

                        _logger.Info($"设备连接成功 (Legacy JSON): {deviceInfo.Name} ({deviceInfo.Host}:{deviceInfo.Port})");
                    }
                }

                if (connected && device != null)
                {
                    // 保存设备实例
                    _devices.TryAdd(deviceId, device);

                    // 更新设备信息
                    deviceInfo.IsConnected = true;
                    deviceInfo.LastConnectedTime = DateTime.Now;
                    deviceInfo.Status = "已连接";

                    // 发布设备创建事件
                    _eventBus.Publish(new MultiDeviceCreateEvent(deviceId, device));
                }

                // 触发设备状态变化事件
                OnDeviceStatusChanged(deviceId, deviceInfo);
                return connected;
            }
            catch (Exception ex)
            {
                _logger.Error($"连接设备时发生异常: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> DisconnectDevice(string deviceId)
        {
            try
            {
                if (!_deviceInfos.TryGetValue(deviceId, out var deviceInfo))
                    return false;

                if (!deviceInfo.IsConnected)
                    return true;

                bool result = true;

                // 断开协议处理器连接
                if (_protocolHandlers.TryGetValue(deviceId, out var protocolHandler))
                {
                    result = await protocolHandler.DisconnectAsync();
                    _protocolHandlers.TryRemove(deviceId, out _);
                    protocolHandler?.Dispose();
                }
                // 或者断开传统通信服务连接
                else if (_communicationServices.TryGetValue(deviceId, out var communicationService))
                {
                    result = await communicationService.DisconnectAsync();
                    _communicationServices.TryRemove(deviceId, out _);
                }

                // 清理设备实例
                _devices.TryRemove(deviceId, out _);

                // 更新设备信息
                deviceInfo.IsConnected = false;
                deviceInfo.Status = result ? "已断开" : "断开失败";

                // 触发设备状态变化事件
                OnDeviceStatusChanged(deviceId, deviceInfo);
                _logger.Info($"设备断开连接: {deviceInfo.Name}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"断开设备连接时发生异常: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> ConnectAllDevices()
        {
            var tasks = _deviceInfos.Values
                .Where(d => !d.IsConnected)
                .Select(async d => await ConnectDevice(d.Id));

            var results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }

        public async Task<bool> DisconnectAllDevices()
        {
            var tasks = _deviceInfos.Values
                .Where(d => d.IsConnected)
                .Select(async d => await DisconnectDevice(d.Id));

            var results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }

        // 添加设备选择功能
        public void SelectDevice(string deviceId)
        {
            if (deviceId != null && !_deviceInfos.ContainsKey(deviceId))
                return;

            var previousId = _selectedDeviceId;
            _selectedDeviceId = deviceId;

            if (previousId != deviceId)
            {
                DeviceSelectionChanged?.Invoke(this, deviceId);
                _logger.Debug($"设备选择已切换: {previousId} -> {deviceId}");
            }
        }

        public string GetSelectedDeviceId() => _selectedDeviceId;

        // 添加疲劳测试状态管理
        public bool IsDeviceTestRunning(string deviceId)
        {
            return _deviceTestStates.TryGetValue(deviceId, out var isRunning) && isRunning;
        }

        public void SetDeviceTestState(string deviceId, bool isRunning)
        {
            _deviceTestStates[deviceId] = isRunning;
        }

        // 添加状态机管理
        public void SetDeviceStateMachine(string deviceId, TestStateMachine stateMachine)
        {
            _deviceStateMachines[deviceId] = stateMachine;
        }

        public TestStateMachine GetDeviceStateMachine(string deviceId)
        {
            return _deviceStateMachines.TryGetValue(deviceId, out var stateMachine) ? stateMachine : null;
        }

        // 添加批量测试操作
        public int StartAllDeviceTests()
        {
            int startedCount = 0;
            foreach (var deviceId in _deviceInfos.Keys.ToList())
            {
                if (_deviceInfos[deviceId].IsConnected && !IsDeviceTestRunning(deviceId))
                {
                    var stateMachine = GetDeviceStateMachine(deviceId);
                    if (stateMachine != null)
                    {
                        try
                        {
                            stateMachine.StartTest();
                            SetDeviceTestState(deviceId, true);
                            startedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"启动设备 {deviceId} 测试失败: {ex.Message}", ex);
                        }
                    }
                }
            }
            return startedCount;
        }

        public int StopAllDeviceTests()
        {
            int stoppedCount = 0;
            foreach (var deviceId in _deviceInfos.Keys.ToList())
            {
                if (IsDeviceTestRunning(deviceId))
                {
                    var stateMachine = GetDeviceStateMachine(deviceId);
                    if (stateMachine != null)
                    {
                        try
                        {
                            stateMachine.StopTest();
                            SetDeviceTestState(deviceId, false);
                            stoppedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"停止设备 {deviceId} 测试失败: {ex.Message}", ex);
                        }
                    }
                }
            }
            return stoppedCount;
        }

        // 添加设备状态栏同步方法
        public void SyncDeviceToStatusBar(DeviceStatusBar statusBar)
        {
            foreach (var deviceInfo in GetAllDevices())
            {
                if (deviceInfo.IsConnected)
                {
                    // 获取设备IP地址
                    string ipAddress = GetDeviceIpAddress(deviceInfo);
                    statusBar.AddDevice(deviceInfo.Id, deviceInfo.Name, ipAddress);
                    statusBar.UpdateDeviceStatus(deviceInfo.Id, MachineStatusType.Idle, true, ipAddress);
                }
            }
        }

        // 获取设备IP地址的辅助方法
        private string GetDeviceIpAddress(DeviceInfo deviceInfo)
        {
            if (deviceInfo.ProtocolConfig != null)
            {
                // 新协议配置
                if (deviceInfo.ProtocolConfig is JsonProtocolConfig jsonConfig)
                {
                    return $"{jsonConfig.Host}:{jsonConfig.Port}";
                }
                else if (deviceInfo.ProtocolConfig is ModbusProtocolConfig modbusConfig)
                {
                    return $"{modbusConfig.Host}:{modbusConfig.Port}";
                }
            }

            // 兼容旧的Host:Port格式
            return $"{deviceInfo.Host}:{deviceInfo.Port}";
        }

        //获取设备协议信息
        public string GetDeviceProtocolInfo(string deviceId)
        {
            if (_deviceInfos.TryGetValue(deviceId, out var deviceInfo))
            {
                if (deviceInfo.ProtocolConfig != null)
                {
                    return $"{deviceInfo.ProtocolType} - {deviceInfo.ProtocolConfig.ConnectionString}";
                }
                else
                {
                    return $"Legacy JSON - {deviceInfo.Host}:{deviceInfo.Port}";
                }
            }
            return "未知协议";
        }

        //切换设备协议
        public async Task<bool> SwitchDeviceProtocol(string deviceId, ProtocolType newProtocolType, ProtocolConfig newConfig)
        {
            if (!_deviceInfos.TryGetValue(deviceId, out var deviceInfo))
                return false;

            // 如果设备已连接，先断开
            bool wasConnected = deviceInfo.IsConnected;
            if (wasConnected)
            {
                await DisconnectDevice(deviceId);
            }

            // 更新协议配置
            deviceInfo.ProtocolType = newProtocolType;
            deviceInfo.ProtocolConfig = newConfig;

            _logger.Info($"设备 {deviceInfo.Name} 协议已切换到: {newProtocolType}");

            // 如果之前已连接，尝试重新连接
            if (wasConnected)
            {
                return await ConnectDevice(deviceId);
            }

            return true;
        }

        public void Dispose()
        {
            _logger.Info($"");
            // 释放所有协议处理器
            foreach (var handler in _protocolHandlers.Values)
            {
                try
                {
                    handler?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Error($"释放协议处理器时发生异常: {ex.Message}", ex);
                }
            }
            _protocolHandlers.Clear();

            // 释放通信服务
            foreach (var service in _communicationServices.Values)
            {
                try
                {
                    service?.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"释放通信服务时发生异常: {ex.Message}", ex);
                }
            }
            _communicationServices.Clear();

            _devices.Clear();
            _deviceInfos.Clear();
        }

        private void OnDeviceConnectionStatusChanged(string deviceId, bool connected)
        {
            if (_deviceInfos.TryGetValue(deviceId, out var deviceInfo))
            {
                deviceInfo.IsConnected = connected;
                deviceInfo.Status = connected ? "已连接" : "已断开";
                OnDeviceStatusChanged(deviceId, deviceInfo);
            }
        }

        private void OnDeviceStatusChanged(string deviceId, DeviceInfo deviceInfo)
        {
            DeviceStatusChanged?.Invoke(this, new DeviceStatusChangedEventArgs(deviceId, deviceInfo));
        }
    }

    public class DeviceStatusChangedEventArgs : EventArgs
    {
        public string DeviceId { get; }
        public DeviceInfo DeviceInfo { get; }

        public DeviceStatusChangedEventArgs(string deviceId, DeviceInfo deviceInfo)
        {
            DeviceId = deviceId;
            DeviceInfo = deviceInfo;
        }
    }
}


