using AntdUI;
using SasTools.Models;
using System;
using System.Windows.Forms;

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

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
