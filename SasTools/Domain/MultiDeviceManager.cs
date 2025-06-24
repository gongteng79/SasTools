using log4net;
using SasTools.Interface;
using System.Collections.Generic;
using System;
using WpFramework.EventBus;
using System.Linq;

namespace SasTools.Domain
{
    public class MultiDeviceManager : IDisposable
    {
        private readonly Dictionary<string, DeviceTestContext> _deviceContexts;
        private readonly FatigueParams _sharedParameters;
        private readonly IEventBus _eventBus;
        private readonly ILog _logger;
        private string _currentSelectedDeviceId;

        public event EventHandler<string> DeviceSelectionChanged;
        public event EventHandler<DeviceTestContext> DeviceAdded;
        public event EventHandler<string> DeviceRemoved;

        public MultiDeviceManager(FatigueParams sharedParameters, IEventBus eventBus)
        {
            _deviceContexts = new Dictionary<string, DeviceTestContext>();
            _sharedParameters = sharedParameters;
            _eventBus = eventBus;
            _logger = LogManager.GetLogger(typeof(MultiDeviceManager));
        }

        public void AddDevice(string deviceId, string deviceName, IDevice device)
        {
            if (_deviceContexts.ContainsKey(deviceId))
                return;

            var context = new DeviceTestContext(deviceId, deviceName, device, _sharedParameters, _eventBus);
            _deviceContexts.Add(deviceId, context);

            DeviceAdded?.Invoke(this, context);
            _logger.Info($"设备已添加到疲劳测试: {deviceName} ({deviceId})");
        }

        public void RemoveDevice(string deviceId)
        {
            if (!_deviceContexts.TryGetValue(deviceId, out var context))
                return;

            // 停止测试并清理资源
            if (context.IsTestRunning)
            {
                context.StateMachine?.StopTest();
            }
            context.DataTable?.Dispose();

            _deviceContexts.Remove(deviceId);

            // 如果移除的是当前选中设备，切换到其他设备
            if (_currentSelectedDeviceId == deviceId)
            {
                var nextDeviceId = _deviceContexts.Keys.FirstOrDefault();
                SelectDevice(nextDeviceId);
            }

            DeviceRemoved?.Invoke(this, deviceId);
            _logger.Info($"设备已从疲劳测试移除: {deviceId}");
        }

        public void SelectDevice(string deviceId)
        {
            if (deviceId != null && !_deviceContexts.ContainsKey(deviceId))
                return;

            var previousId = _currentSelectedDeviceId;
            _currentSelectedDeviceId = deviceId;

            if (previousId != deviceId)
            {
                DeviceSelectionChanged?.Invoke(this, deviceId);
                _logger.Debug($"设备选择已切换: {previousId} -> {deviceId}");
            }
        }

        public DeviceTestContext GetCurrentDevice()
        {
            return _currentSelectedDeviceId != null && _deviceContexts.TryGetValue(_currentSelectedDeviceId, out var context)
                ? context : null;
        }

        public DeviceTestContext GetDevice(string deviceId)
        {
            return _deviceContexts.TryGetValue(deviceId, out var context) ? context : null;
        }

        public IEnumerable<DeviceTestContext> GetAllDevices()
        {
            return _deviceContexts.Values;
        }

        public string GetCurrentDeviceId()
        {
            return _currentSelectedDeviceId;
        }

        public void Dispose()
        {
            foreach (var context in _deviceContexts.Values)
            {
                context.StateMachine?.StopTest();
                context.DataTable?.Dispose();
            }
            _deviceContexts.Clear();
        }
    }
}
