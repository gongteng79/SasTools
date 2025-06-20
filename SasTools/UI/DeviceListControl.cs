using AntdUI;
using SasTools.Models;
using SasTools.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SasTools.UI.Controls
{
    public partial class DeviceListControl : UserControl
    {
        private DeviceManager _deviceManager;
        private string _selectedDeviceId;

        public event EventHandler<string> DeviceSelected;
        public event EventHandler<string> DeviceConnectRequested;
        public event EventHandler<string> DeviceDisconnectRequested;

        public DeviceListControl()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            this.btnAddDevice.Click += BtnAddDevice_Click;
            this.btnRemoveDevice.Click += BtnRemoveDevice_Click;
            this.btnConnectAll.Click += BtnConnectAll_Click;
            this.btnDisconnectAll.Click += BtnDisconnectAll_Click;
        }

        public void SetDeviceManager(DeviceManager deviceManager)
        {
            // 如果已有设备管理器，先取消事件订阅
            if (_deviceManager != null)
            {
                _deviceManager.DeviceStatusChanged -= OnDeviceStatusChanged;
            }

            _deviceManager = deviceManager;//设置新的设备管理器
            if (_deviceManager != null)
            {
                _deviceManager.DeviceStatusChanged += OnDeviceStatusChanged;
                RefreshDeviceList();//刷新设备列表
            }
        }

        private void RefreshDeviceList()
        {
            if (_deviceManager == null) return;//安全检查

            var devices = _deviceManager.GetAllDevices().ToList();//获取所有设备

            // 清空现有的设备表格
            this.deviceTable.DataSource = null;

            // 创建简单的设备列表
            CreateSimpleDeviceList(devices);
        }

        private System.Windows.Forms.Panel _currentDevicePanel;

        private void CreateSimpleDeviceList(List<DeviceInfo> devices)
        {
            // 如果已经有设备面板，先移除
            if (_currentDevicePanel != null)
            {
                this.tableLayoutPanel1.Controls.Remove(_currentDevicePanel);
                _currentDevicePanel.Dispose();
            }

            // 创建新的设备面板，使用FlowLayoutPanel确保顺序
            _currentDevicePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };

            // 添加新的Panel
            this.tableLayoutPanel1.Controls.Add(_currentDevicePanel, 0, 1);

            // 按照添加顺序创建设备面板
            foreach (var device in devices)
            {
                var devicePanel = CreateDevicePanel(device);
                _currentDevicePanel.Controls.Add(devicePanel);
            }
        }

        private System.Windows.Forms.Panel CreateDevicePanel(DeviceInfo device)
        {
            var panel = new System.Windows.Forms.Panel
            {
                Size = new Size(750, 50),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            var isSelected = device.Id == _selectedDeviceId;

            // 设备名称
            var lblName = new AntdUI.Label
            {
                Text = isSelected ? $"★ {device.Name}" : device.Name,
                Location = new Point(10, 15),
                Size = new Size(150, 20),
                Font = new Font("微软雅黑", 10, isSelected ? FontStyle.Bold : FontStyle.Regular)
            };

            // 地址
            var lblAddress = new AntdUI.Label
            {
                Text = $"{device.Host}:{device.Port}",
                Location = new Point(170, 15),
                Size = new Size(150, 20)
            };

            // 状态
            var lblStatus = new AntdUI.Label
            {
                Text = device.IsConnected ? "已连接" : "未连接",
                Location = new Point(330, 15),
                Size = new Size(80, 20),
                ForeColor = device.IsConnected ? Color.Green : Color.Gray
            };

            // 连接按钮
            var btnConnect = new AntdUI.Button
            {
                Text = device.IsConnected ? "断开" : "连接",
                Location = new Point(420, 10),
                Size = new Size(60, 30),
                Type = device.IsConnected ? TTypeMini.Warn : TTypeMini.Primary,
                Tag = device.Id
            };
            btnConnect.Click += (s, e) =>
            {
                if (device.IsConnected)
                    DeviceDisconnectRequested?.Invoke(this, device.Id);
                else
                    DeviceConnectRequested?.Invoke(this, device.Id);
            };

            // 选择按钮
            var btnSelect = new AntdUI.Button
            {
                Text = isSelected ? "取消" : "选择",
                Location = new Point(490, 10),
                Size = new Size(60, 30),
                Type = isSelected ? TTypeMini.Success : TTypeMini.Default,
                Tag = device.Id
            };
            btnSelect.Click += (s, e) =>
            {
                if (isSelected)
                {
                    // 取消选择
                    _selectedDeviceId = null;
                    DeviceSelected?.Invoke(this, null);
                }
                else
                {
                    // 选择设备
                    _selectedDeviceId = device.Id;
                    DeviceSelected?.Invoke(this, device.Id);
                }
                RefreshDeviceList(); // 刷新以显示选中状态
            };

            panel.Controls.AddRange(new Control[] { lblName, lblAddress, lblStatus, btnConnect, btnSelect });
            return panel;
        }



        public string GetSelectedDeviceId()
        {
            return _selectedDeviceId;
        }

        public void SetSelectedDeviceId(string deviceId)
        {
            _selectedDeviceId = deviceId;
            RefreshDeviceList();
        }

        private void BtnAddDevice_Click(object sender, EventArgs e)
        {
            var deviceInfo = AddDeviceDialog.ShowAddDeviceDialog(this.FindForm());
            if (deviceInfo != null)
            {
                if (_deviceManager.AddDevice(deviceInfo))
                {
                    RefreshDeviceList();
                    AntdUI.Message.success(this.FindForm(), "设备添加成功");
                }
                else
                {
                    AntdUI.Message.error(this.FindForm(), "设备添加失败");
                }
            }
        }

        private async void BtnRemoveDevice_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedDeviceId))
            {
                AntdUI.Message.info(this.FindForm(), "请先选择要移除的设备");
                return;
            }

            var deviceInfo = _deviceManager.GetDevice(_selectedDeviceId);
            if (deviceInfo == null)
            {
                AntdUI.Message.error(this.FindForm(), "设备不存在");
                return;
            }

            // 确认对话框
            var result = MessageBox.Show($"确定要移除设备 '{deviceInfo.Name}' 吗？",
                                        "确认移除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                bool success = await _deviceManager.RemoveDevice(_selectedDeviceId);
                if (success)
                {
                    _selectedDeviceId = null;
                    DeviceSelected?.Invoke(this, null); // 通知主界面清除选择
                    RefreshDeviceList();
                    AntdUI.Message.success(this.FindForm(), "设备移除成功");
                }
                else
                {
                    AntdUI.Message.error(this.FindForm(), "设备移除失败");
                }
            }
        }

        private async void BtnConnectAll_Click(object sender, EventArgs e)
        {
            this.btnConnectAll.Loading = true;
            try
            {
                bool result = await _deviceManager.ConnectAllDevices();
                string message = result ? "所有设备连接成功" : "部分设备连接失败";
                AntdUI.Message.info(this.FindForm(), message);
            }
            finally
            {
                this.btnConnectAll.Loading = false;
            }
        }

        private async void BtnDisconnectAll_Click(object sender, EventArgs e)
        {
            this.btnDisconnectAll.Loading = true;
            try
            {
                bool result = await _deviceManager.DisconnectAllDevices();
                string message = result ? "所有设备断开成功" : "部分设备断开失败";
                AntdUI.Message.info(this.FindForm(), message);
            }
            finally
            {
                this.btnDisconnectAll.Loading = false;
            }
        }



        private void OnDeviceStatusChanged(object sender, DeviceStatusChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => RefreshDeviceList()));
            }
            else
            {
                RefreshDeviceList();
            }
        }
    }
}

