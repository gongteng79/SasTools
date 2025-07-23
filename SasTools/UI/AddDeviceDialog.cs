using AntdUI;
using SasTools.Models;
using System;
using System.Windows.Forms;
using SasTools.Models.Protocol;

namespace SasTools.UI
{
    public partial class AddDeviceDialog : Form
    {
        public DeviceInfo DeviceInfo { get; private set; }

        public AddDeviceDialog()
        {
            InitializeComponent();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            this.btnOK.Click += BtnOK_Click;
            this.btnCancel.Click += BtnCancel_Click;
            this.cmbProtocolType.SelectedIndexChanged += CmbProtocolType_SelectedIndexChanged;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            DeviceInfo = new DeviceInfo
            {
                Name = this.txtDeviceName.Text.Trim(),
                Host = this.txtHost.Text.Trim(),
                Port = (int)this.numPort.Value
            };

            // 根据协议类型设置协议配置
            if (this.cmbProtocolType.SelectedIndex == 0) // JSON协议
            {
                DeviceInfo.SetJsonProtocol(DeviceInfo.Host, DeviceInfo.Port);
            }
            else if (this.cmbProtocolType.SelectedIndex == 1) // Modbus TCP
            {
                DeviceInfo.SetModbusProtocol(DeviceInfo.Host, DeviceInfo.Port, (byte)this.numSlaveId.Value);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CmbProtocolType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 根据协议类型显示/隐藏SlaveId控件
            bool isModbus = this.cmbProtocolType.SelectedIndex == 1; // 1表示Modbus TCP

            this.lblSlaveId.Visible = isModbus;
            this.numSlaveId.Visible = isModbus;

            // 自动切换默认端口
            if (isModbus)
            {
                // Modbus TCP默认端口1502
                this.numPort.Value = 1502;
                this.txtHost.Text = "192.168.1.12";
            }
            else
            {
                // JSON协议默认端口6062
                this.numPort.Value = 6062;
                this.txtHost.Text = "192.168.1.12";
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(this.txtDeviceName.Text))
            {
                AntdUI.Message.error(this, "请输入设备名称");
                this.txtDeviceName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.txtHost.Text))
            {
                AntdUI.Message.error(this, "请输入主机地址");
                this.txtHost.Focus();
                return false;
            }

            if (this.cmbProtocolType.SelectedIndex < 0)
            {
                AntdUI.Message.error(this, "请选择通信协议");
                this.cmbProtocolType.Focus();
                return false;
            }
            // 如果选择Modbus协议，验证SlaveId
            if (this.cmbProtocolType.SelectedIndex == 1 && this.numSlaveId.Value < 1)
            {
                AntdUI.Message.error(this, "请输入有效的从站ID (1-255)");
                this.numSlaveId.Focus();
                return false;
            }
            return true;
        }

        public static DeviceInfo ShowAddDeviceDialog(IWin32Window owner = null)
        {
            using (var dialog = new AddDeviceDialog())
            {
                if (dialog.ShowDialog(owner) == DialogResult.OK)
                {
                    return dialog.DeviceInfo;
                }
                return null;
            }
        }
    }
}
