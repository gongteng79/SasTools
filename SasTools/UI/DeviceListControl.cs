using AntdUI;
using SasTools.Models;
using SasTools.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SasTools.Models.Protocol;

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
                Size = new Size(850, 50),
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

            // 协议类型
            var lblProtocol = new AntdUI.Label
            {
                Text = GetProtocolDisplayText(device),
                Location = new Point(530, 15),
                Size = new Size(100, 20),
                ForeColor = GetProtocolColor(device.ProtocolType)
            };

            // 连接按钮
            var btnConnect = new AntdUI.Button
            {
                Text = device.IsConnected ? "断开" : "连接",
                Location = new Point(420, 10),
                Size = new Size(50, 30),
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
                Location = new Point(620, 10),
                Size = new Size(60, 30),
                Type = isSelected ? TTypeMini.Success : TTypeMini.Default,
                Tag = device.Id
            };
            // 切换协议按钮
            var btnSwitchProtocol = new AntdUI.Button
            {
                Text = "切换协议",
                Location = new Point(700, 10),
                Size = new Size(70, 30),
                Type = TTypeMini.Default,
                Tag = device.Id
            };
            btnSwitchProtocol.Click += (s, e) => ShowProtocolSwitchDialog(device);

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
            panel.Controls.AddRange(new Control[] { lblName, lblAddress, lblStatus, lblProtocol, btnConnect, btnSelect, btnSwitchProtocol });
            return panel;
        }

        private async void ShowProtocolSwitchDialog(DeviceInfo device)
        {
            // 创建简单的协议切换对话框
            using (var dialog = new Form())
            {
                dialog.Text = $"切换设备协议 - {device.Name}";
                dialog.Size = new Size(400, 200);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var lblCurrent = new AntdUI.Label
                {
                    Text = $"当前协议: {GetProtocolDisplayText(device)}",
                    Location = new Point(20, 20),
                    Size = new Size(300, 25)
                };

                var lblNew = new AntdUI.Label
                {
                    Text = "切换到:",
                    Location = new Point(20, 60),
                    Size = new Size(80, 25)
                };

                var cmbNewProtocol = new AntdUI.Select
                {
                    Location = new Point(100, 55),
                    Size = new Size(150, 30)
                };
                cmbNewProtocol.Items.Add("JSON协议");
                cmbNewProtocol.Items.Add("Modbus TCP");
                cmbNewProtocol.SelectedIndex = device.ProtocolType == Models.Protocol.ProtocolType.Json ? 1 : 0; // 选择相反的协议

                var lblSlaveId = new AntdUI.Label
                {
                    Text = "从站ID:",
                    Location = new Point(270, 60),
                    Size = new Size(60, 25),
                    Visible = cmbNewProtocol.SelectedIndex == 1
                };

                var numSlaveId = new AntdUI.InputNumber
                {
                    Location = new Point(330, 55),
                    Size = new Size(50, 30),
                    Minimum = 1,
                    Maximum = 255,
                    Value = 1,
                    Visible = cmbNewProtocol.SelectedIndex == 1
                };

                // 协议选择变化事件
                cmbNewProtocol.SelectedIndexChanged += (s, e) =>
                {
                    bool isModbus = cmbNewProtocol.SelectedIndex == 1;
                    lblSlaveId.Visible = isModbus;
                    numSlaveId.Visible = isModbus;
                };

                var btnOK = new AntdUI.Button
                {
                    Text = "确定",
                    Location = new Point(200, 120),
                    Size = new Size(75, 30),
                    Type = TTypeMini.Primary
                };

                var btnCancel = new AntdUI.Button
                {
                    Text = "取消",
                    Location = new Point(285, 120),
                    Size = new Size(75, 30),
                    Type = TTypeMini.Default
                };

                btnOK.Click += async (s, e) =>
                {
                    try
                    {
                        Models.Protocol.ProtocolType newProtocolType;
                        Models.Protocol.ProtocolConfig newConfig;

                        if (cmbNewProtocol.SelectedIndex == 0) // JSON
                        {
                            newProtocolType = Models.Protocol.ProtocolType.Json;
                            newConfig = new Models.Protocol.JsonProtocolConfig
                            {
                                Host = device.Host,
                                Port = device.Port
                            };
                        }
                        else // Modbus
                        {
                            newProtocolType = Models.Protocol.ProtocolType.ModbusTcp;
                            newConfig = new Models.Protocol.ModbusProtocolConfig
                            {
                                Host = device.Host,
                                Port = device.Port,
                                SlaveId = (byte)numSlaveId.Value
                            };
                        }

                        bool success = await _deviceManager.SwitchDeviceProtocol(device.Id, newProtocolType, newConfig);
                        if (success)
                        {
                            AntdUI.Message.success(this.FindForm(), "协议切换成功");
                            RefreshDeviceList();
                            dialog.DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            AntdUI.Message.error(this.FindForm(), "协议切换失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        AntdUI.Message.error(this.FindForm(), $"协议切换出错: {ex.Message}");
                    }
                };

                btnCancel.Click += (s, e) => dialog.DialogResult = DialogResult.Cancel;

                dialog.Controls.AddRange(new Control[] { lblCurrent, lblNew, cmbNewProtocol, lblSlaveId, numSlaveId, btnOK, btnCancel });
                dialog.ShowDialog(this.FindForm());
            }
        }
        private string GetProtocolDisplayText(DeviceInfo device)
        {
            if (device.ProtocolConfig != null)
            {
                switch (device.ProtocolType)
                {
                    case Models.Protocol.ProtocolType.Json:
                        return "JSON";
                    case Models.Protocol.ProtocolType.ModbusTcp:
                        return $"Modbus({((Models.Protocol.ModbusProtocolConfig)device.ProtocolConfig).SlaveId})";
                    default:
                        return "未知";
                }
            }
            return "JSON(旧)";
        }

        private Color GetProtocolColor(Models.Protocol.ProtocolType protocolType)
        {
            switch (protocolType)
            {
                case Models.Protocol.ProtocolType.Json:
                    return Color.Blue;
                case Models.Protocol.ProtocolType.ModbusTcp:
                    return Color.Orange;
                default:
                    return Color.Gray;
            }
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

