using AntdUI;

namespace SasTools.UI
{
    partial class AddDeviceDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private AntdUI.Label lblDeviceName;
        private AntdUI.Input txtDeviceName;
        private AntdUI.Label lblHost;
        private AntdUI.Input txtHost;
        private AntdUI.Label lblPort;
        private AntdUI.InputNumber numPort;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private AntdUI.Button btnOK;
        private AntdUI.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDeviceName = new AntdUI.Label();
            this.txtDeviceName = new AntdUI.Input();
            this.lblHost = new AntdUI.Label();
            this.txtHost = new AntdUI.Input();
            this.lblPort = new AntdUI.Label();
            this.numPort = new AntdUI.InputNumber();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOK = new AntdUI.Button();
            this.btnCancel = new AntdUI.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();

            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblDeviceName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtDeviceName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblHost, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtHost, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPort, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.numPort, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(364, 150);
            this.tableLayoutPanel1.TabIndex = 0;

            // 
            // lblDeviceName
            // 
            this.lblDeviceName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDeviceName.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblDeviceName.Location = new System.Drawing.Point(3, 3);
            this.lblDeviceName.Name = "lblDeviceName";
            this.lblDeviceName.Size = new System.Drawing.Size(94, 44);
            this.lblDeviceName.TabIndex = 0;
            this.lblDeviceName.Text = "设备名称:";
            this.lblDeviceName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // txtDeviceName
            // 
            this.txtDeviceName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDeviceName.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtDeviceName.Location = new System.Drawing.Point(103, 10);
            this.txtDeviceName.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.txtDeviceName.Name = "txtDeviceName";
            this.txtDeviceName.PlaceholderText = "请输入设备名称";
            this.txtDeviceName.Size = new System.Drawing.Size(258, 30);
            this.txtDeviceName.TabIndex = 1;

            // 
            // lblHost
            // 
            this.lblHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHost.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblHost.Location = new System.Drawing.Point(3, 53);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(94, 44);
            this.lblHost.TabIndex = 2;
            this.lblHost.Text = "主机地址:";
            this.lblHost.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            //
            // txtHost
            //
            this.txtHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHost.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtHost.Location = new System.Drawing.Point(103, 60);
            this.txtHost.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.txtHost.Name = "txtHost";
            this.txtHost.PlaceholderText = "请输入IP地址";
            this.txtHost.Size = new System.Drawing.Size(258, 30);
            this.txtHost.TabIndex = 3;
            this.txtHost.Text = "192.168.2.12";

            //
            // lblPort
            //
            this.lblPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPort.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.lblPort.Location = new System.Drawing.Point(3, 103);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(94, 44);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "端口:";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            //
            // numPort
            //
            this.numPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numPort.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.numPort.Location = new System.Drawing.Point(103, 110);
            this.numPort.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(258, 30);
            this.numPort.TabIndex = 5;
            this.numPort.Value = new decimal(new int[] { 6062, 0, 0, 0 });

            //
            // flowLayoutPanel1
            //
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnOK);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 160);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(364, 50);
            this.flowLayoutPanel1.TabIndex = 1;

            //
            // btnCancel
            //
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnCancel.Location = new System.Drawing.Point(286, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.Type = AntdUI.TTypeMini.Default;

            //
            // btnOK
            //
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnOK.Location = new System.Drawing.Point(205, 13);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 30);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.Type = AntdUI.TTypeMini.Primary;

            //
            // AddDeviceDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 220);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddDeviceDialog";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "添加设备";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}