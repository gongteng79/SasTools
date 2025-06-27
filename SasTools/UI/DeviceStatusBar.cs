using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SasTools.Domain;
using SasTools.Events;

namespace SasTools.UI
{
    public partial class DeviceStatusBar : UserControl
    {
        private Dictionary<string, AntdUI.Button> _deviceButtons;
        private string _selectedDeviceId;

        // 事件定义
        public event EventHandler<string> DeviceSelected;
        public event EventHandler StartAllClicked;
        public event EventHandler StopAllClicked;

        public DeviceStatusBar()
        {
            InitializeComponent();
            InitializeDeviceButtons();
            SetupEventHandlers();
        }

        private void InitializeDeviceButtons()
        {
            _deviceButtons = new Dictionary<string, AntdUI.Button>();
        }

        private void SetupEventHandlers()
        {
            // 批量操作按钮事件
            btnStartAll.Click += (s, e) => StartAllClicked?.Invoke(this, EventArgs.Empty);
            btnStopAll.Click += (s, e) => StopAllClicked?.Invoke(this, EventArgs.Empty);
        }

        // 添加设备到状态栏
        public void AddDevice(string deviceId, string deviceName)
        {
            if (_deviceButtons.ContainsKey(deviceId))
                return;

            var deviceButton = new AntdUI.Button
            {
                Name = $"btnDevice_{deviceId}",
                Text = deviceName,
                Size = new Size(60, 20),
                Type = AntdUI.TTypeMini.Default,
                Font = new Font("微软雅黑", 9F),
                Tag = deviceId
            };

            // 设备按钮点击事件
            deviceButton.Click += DeviceButton_Click;

            // 添加到容器和字典
            _deviceButtons.Add(deviceId, deviceButton);
            flowPanelDevices.Controls.Add(deviceButton);
        }

        //从状态栏移除设备
        public void RemoveDevice(string deviceId)
        {
            if (_deviceButtons.TryGetValue(deviceId, out var button))
            {
                //如果移除的是当前选中的设备，清楚选中状态
                if (_selectedDeviceId == deviceId)
                {
                    _selectedDeviceId = null;
                }

                //从UI容器中移除按钮
                flowPanelDevices.Controls.Remove(button);

                //从字典移除
                _deviceButtons.Remove(deviceId);

                //释放按钮资源
                button.Dispose();
            }
        }

        // 更新设备状态
        public void UpdateDeviceStatus(string deviceId, MachineStatusType status, bool isConnected)
        {
            if (_deviceButtons.TryGetValue(deviceId, out var button))
            {
                if (!isConnected)
                {
                    button.Type = AntdUI.TTypeMini.Default;
                    button.Loading = false;
                }
                else
                {
                    switch (status)
                    {
                        case MachineStatusType.Idle:
                            button.Type = AntdUI.TTypeMini.Primary;
                            button.Loading = false;
                            break;
                        case MachineStatusType.Normal:
                        case MachineStatusType.Forward:
                        case MachineStatusType.Reverse:
                        case MachineStatusType.Waiting:
                            button.Type = AntdUI.TTypeMini.Success;
                            button.Loading = true;
                            break;
                        case MachineStatusType.Error:
                            button.Type = AntdUI.TTypeMini.Error;
                            button.Loading = false;
                            break;
                    }
                }
            }
        }

        // 设置选中设备
        public void SelectDevice(string deviceId)
        {
            // 清除之前的选中状态
            if (!string.IsNullOrEmpty(_selectedDeviceId) && _deviceButtons.TryGetValue(_selectedDeviceId, out var prevButton))
            {
                UpdateButtonSelectedState(prevButton, false);
            }

            _selectedDeviceId = deviceId;

            // 设置新的选中状态
            if (!string.IsNullOrEmpty(deviceId) && _deviceButtons.TryGetValue(deviceId, out var currentButton))
            {
                UpdateButtonSelectedState(currentButton, true);
            }
        }

        private void UpdateButtonSelectedState(AntdUI.Button button, bool selected)
        {
            if (selected)
            {
                // 选中状态：添加边框
                button.BorderWidth = 2;
            }
            else
            {
                // 非选中状态：正常边框
                button.BorderWidth = 1;
            }
        }

        private void DeviceButton_Click(object sender, EventArgs e)
        {
            if (sender is AntdUI.Button button && button.Tag is string deviceId)
            {
                SelectDevice(deviceId);
                DeviceSelected?.Invoke(this, deviceId);
            }
        }

        // 获取当前选中的设备ID
        public string GetSelectedDeviceId()
        {
            return _selectedDeviceId;
        }

        // 获取所有设备按钮
        public IEnumerable<string> GetAllDeviceIds()
        {
            return _deviceButtons.Keys;
        }
    }
}
