using AntdUI;
using SasTools.Services;
using SasTools.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WpFramework.EventBus;

namespace SasTools.UI
{
    public partial class DeviceManagementView : UserControl
    {
        private DeviceManager _deviceManager;
        private DeviceListControl _deviceListControl;
        private IEventBus _eventBus;

        public event EventHandler<string> DeviceSelected;//设备选择事件
        public event EventHandler<string> DeviceSelectedForConnection;//设备连接选择事件

        public DeviceManagementView(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));// 检查事件总线是否为null
            InitializeComponent();
            InitializeDeviceManager();//初始化设备管理器
            InitializeDeviceListControl();// 初始化设备列表控件
        }

        private void InitializeDeviceManager()
        {
            _deviceManager = new DeviceManager(_eventBus);// 创建设备管理器实例并传入事件总线
        }

        private void InitializeDeviceListControl()
        {
            _deviceListControl = new DeviceListControl // 创建设备管理器实例并传入事件总线
            {
                Dock = DockStyle.Fill
            };

            // 设置设备管理器并订阅事件
            _deviceListControl.SetDeviceManager(_deviceManager);
            _deviceListControl.DeviceSelected += OnDeviceSelected;
            _deviceListControl.DeviceConnectRequested += OnDeviceConnectRequested;
            _deviceListControl.DeviceDisconnectRequested += OnDeviceDisconnectRequested;

            this.panelContent.Controls.Add(_deviceListControl);// 将设备列表控件添加到内容面板
        }

        // 设备选择事件处理
        private void OnDeviceSelected(object sender, string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                // 取消选择
                AntdUI.Message.info(this.FindForm(), "已取消设备选择");
                DeviceSelected?.Invoke(this, deviceId);// 触发设备选择事件
                DeviceSelectedForConnection?.Invoke(this, deviceId);// 触发设备连接选择事件
                return;
            }

            var deviceInfo = _deviceManager.GetDevice(deviceId); // 获取设备信息
            if (deviceInfo != null)
            {
                AntdUI.Message.success(this.FindForm(), $"已选择设备: {deviceInfo.Name}");
                DeviceSelected?.Invoke(this, deviceId);// 触发设备选择事件

                // 通知主界面更新连接按钮状态
                DeviceSelectedForConnection?.Invoke(this, deviceId);
            }   
        }


        // 设备连接请求处理
        private async void OnDeviceConnectRequested(object sender, string deviceId)
        {
            try
            {
                bool result = await _deviceManager.ConnectDevice(deviceId);
                var deviceInfo = _deviceManager.GetDevice(deviceId);
                string message = result ? "设备连接成功" : "设备连接失败";

                if (result)
                {
                    AntdUI.Message.success(this.FindForm(), message);// 成功提示
                }
                else
                {
                    AntdUI.Message.error(this.FindForm(), message);// 失败提示
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), $"连接设备时发生错误: {ex.Message}");
            }
        }

        //设备断开请求处理
        private async void OnDeviceDisconnectRequested(object sender, string deviceId)
        {
            try
            {
                bool result = await _deviceManager.DisconnectDevice(deviceId);
                var deviceInfo = _deviceManager.GetDevice(deviceId);
                string message = result ? "设备断开成功" : "设备断开失败";

                AntdUI.Message.info(this.FindForm(), message);
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), $"断开设备时发生错误: {ex.Message}");
            }
        }

        // 获取设备管理器实例
        public DeviceManager GetDeviceManager()
        {
            return _deviceManager;
        }

        // 获取当前选中的设备ID
        public string GetSelectedDeviceId()
        {
            return _deviceListControl?.GetSelectedDeviceId();
        }
    }
}
