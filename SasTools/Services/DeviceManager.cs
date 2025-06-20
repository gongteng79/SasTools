using log4net;
using SasTools.Events;
using SasTools.Interface;
using SasTools.Models;
using SasTools.Models.SasModule;
using SasTools.Services;
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
        private readonly List<string> _deviceOrder; // 跟踪设备添加顺序
        private readonly object _orderLock = new object(); // 保护顺序列表的锁

        public DeviceManager(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _deviceInfos = new ConcurrentDictionary<string, DeviceInfo>();
            _devices = new ConcurrentDictionary<string, IDevice>();
            _communicationServices = new ConcurrentDictionary<string, ICommunicationService>();
            _deviceOrder = new List<string>();
        }

        public event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;

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

            // 从顺序列表中移除
            lock (_orderLock)
            {
                _deviceOrder.Remove(deviceId);
            }

            return true;
        }

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

                // 创建通信服务
                var communicationService = new CommunicationService(deviceInfo.Host, deviceInfo.Port);
                communicationService.ConnectionStatusChanged += (sender, isConnected) =>
                {
                    OnDeviceConnectionStatusChanged(deviceId, isConnected);
                };

                // 尝试真实连接
                bool connected = await communicationService.ConnectAsync();

                if (connected)
                {
                    // 创建设备实例
                    var device = new SasDevice(communicationService, _eventBus);

                    // 保存实例
                    _communicationServices.TryAdd(deviceId, communicationService);
                    _devices.TryAdd(deviceId, device);

                    // 更新设备信息
                    deviceInfo.IsConnected = true;
                    deviceInfo.LastConnectedTime = DateTime.Now;
                    deviceInfo.Status = "已连接";

                    // 发布设备创建事件
                    _eventBus.Publish(new MultiDeviceCreateEvent(deviceId, device));

                    _logger.Info($"设备连接成功: {deviceInfo.Name} ({deviceInfo.Host}:{deviceInfo.Port})");
                }
                else
                {
                    deviceInfo.Status = "连接失败";
                    _logger.Error($"设备连接失败: {deviceInfo.Name} ({deviceInfo.Host}:{deviceInfo.Port})");
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

                // 模拟断开延迟
                await Task.Delay(500);

                // 模拟断开成功
                bool result = true;

                // 清理资源（如果有真实连接的话）
                _communicationServices.TryRemove(deviceId, out _);
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


