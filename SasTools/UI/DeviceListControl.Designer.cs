using AntdUI;

namespace SasTools.UI.Controls
{
    partial class DeviceListControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel toolbarPanel;
        private AntdUI.Button btnAddDevice;
        private AntdUI.Button btnRemoveDevice;
        private AntdUI.Button btnConnectAll;
        private AntdUI.Button btnDisconnectAll;
        private AntdUI.Table deviceTable;

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
            this.toolbarPanel = new System.Windows.Forms.Panel();
            this.btnAddDevice = new AntdUI.Button();
            this.btnRemoveDevice = new AntdUI.Button();
            this.btnConnectAll = new AntdUI.Button();
            this.btnDisconnectAll = new AntdUI.Button();
            this.deviceTable = new AntdUI.Table();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolbarPanel.SuspendLayout();
            this.SuspendLayout();

            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolbarPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.deviceTable, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 600);
            this.tableLayoutPanel1.TabIndex = 0;

            // 
            // toolbarPanel
            // 
            this.toolbarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.toolbarPanel.Controls.Add(this.btnDisconnectAll);
            this.toolbarPanel.Controls.Add(this.btnConnectAll);
            this.toolbarPanel.Controls.Add(this.btnRemoveDevice);
            this.toolbarPanel.Controls.Add(this.btnAddDevice);
            this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolbarPanel.Location = new System.Drawing.Point(3, 3);
            this.toolbarPanel.Name = "toolbarPanel";
            this.toolbarPanel.Size = new System.Drawing.Size(794, 44);
            this.toolbarPanel.TabIndex = 0;

            // 
            // btnAddDevice
            // 
            this.btnAddDevice.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnAddDevice.Location = new System.Drawing.Point(10, 7);
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.Size = new System.Drawing.Size(80, 30);
            this.btnAddDevice.TabIndex = 0;
            this.btnAddDevice.Text = "添加设备";
            this.btnAddDevice.Type = AntdUI.TTypeMini.Primary;

            // 
            // btnRemoveDevice
            // 
            this.btnRemoveDevice.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnRemoveDevice.Location = new System.Drawing.Point(100, 7);
            this.btnRemoveDevice.Name = "btnRemoveDevice";
            this.btnRemoveDevice.Size = new System.Drawing.Size(80, 30);
            this.btnRemoveDevice.TabIndex = 1;
            this.btnRemoveDevice.Text = "移除设备";
            this.btnRemoveDevice.Type = AntdUI.TTypeMini.Default;

            // 
            // btnConnectAll
            // 
            this.btnConnectAll.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnConnectAll.Location = new System.Drawing.Point(190, 7);
            this.btnConnectAll.Name = "btnConnectAll";
            this.btnConnectAll.Size = new System.Drawing.Size(80, 30);
            this.btnConnectAll.TabIndex = 2;
            this.btnConnectAll.Text = "连接所有";
            this.btnConnectAll.Type = AntdUI.TTypeMini.Success;

            // 
            // btnDisconnectAll
            // 
            this.btnDisconnectAll.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnDisconnectAll.Location = new System.Drawing.Point(280, 7);
            this.btnDisconnectAll.Name = "btnDisconnectAll";
            this.btnDisconnectAll.Size = new System.Drawing.Size(80, 30);
            this.btnDisconnectAll.TabIndex = 3;
            this.btnDisconnectAll.Text = "断开所有";
            this.btnDisconnectAll.Type = AntdUI.TTypeMini.Warn;

            //
            // deviceTable
            //
            this.deviceTable.Bordered = true;
            this.deviceTable.Columns = new AntdUI.ColumnCollection(new AntdUI.Column[] {
                new AntdUI.Column("name", "设备名称") { Width = "150" },
                new AntdUI.Column("address", "地址") { Width = "200" },
                new AntdUI.Column("status", "状态") { Width = "100" },
                new AntdUI.Column("lastConnected", "最后连接") { Width = "150" },
                new AntdUI.Column("actions", "操作") { Width = "200" }
            });
            this.deviceTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceTable.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.deviceTable.Location = new System.Drawing.Point(3, 53);
            this.deviceTable.Name = "deviceTable";
            this.deviceTable.Size = new System.Drawing.Size(794, 544);
            this.deviceTable.TabIndex = 1;

            //
            // DeviceListControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DeviceListControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.toolbarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

