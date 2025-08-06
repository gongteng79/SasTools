namespace SasTools.UI
{
    partial class DeviceStatusBar
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.flowPanelBatchOps = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStartAll = new AntdUI.Button();
            this.btnStopAll = new AntdUI.Button();
            this.flowPanelDevices = new System.Windows.Forms.FlowLayoutPanel();
            this.panelMain.SuspendLayout();
            this.flowPanelBatchOps.SuspendLayout();
            this.flowPanelDevices.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.flowPanelBatchOps);
            this.panelMain.Controls.Add(this.flowPanelDevices);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(800, 50);
            this.panelMain.TabIndex = 0;
            // 
            // flowPanelBatchOps
            // 
            this.flowPanelBatchOps.Controls.Add(this.btnStartAll);
            this.flowPanelBatchOps.Controls.Add(this.btnStopAll);
            this.flowPanelBatchOps.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowPanelBatchOps.Location = new System.Drawing.Point(600, 0);
            this.flowPanelBatchOps.Name = "flowPanelBatchOps";
            this.flowPanelBatchOps.Padding = new System.Windows.Forms.Padding(5);
            this.flowPanelBatchOps.Size = new System.Drawing.Size(200, 50);
            this.flowPanelBatchOps.TabIndex = 1;
            // 
            // btnStartAll
            // 
            this.btnStartAll.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStartAll.Location = new System.Drawing.Point(8, 8);
            this.btnStartAll.Name = "btnStartAll";
            this.btnStartAll.Size = new System.Drawing.Size(88, 39);
            this.btnStartAll.TabIndex = 0;
            this.btnStartAll.Text = "全部启动";
            this.btnStartAll.Type = AntdUI.TTypeMini.Success;
            // 
            // btnStopAll
            // 
            this.btnStopAll.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStopAll.Location = new System.Drawing.Point(102, 8);
            this.btnStopAll.Name = "btnStopAll";
            this.btnStopAll.Size = new System.Drawing.Size(85, 40);
            this.btnStopAll.TabIndex = 1;
            this.btnStopAll.Text = "全部停止";
            this.btnStopAll.Type = AntdUI.TTypeMini.Error;
            // 
            // flowPanelDevices
            // 
            this.flowPanelDevices.AutoScroll = true;
            this.flowPanelDevices.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowPanelDevices.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowPanelDevices.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.flowPanelDevices.Location = new System.Drawing.Point(0, 0);
            this.flowPanelDevices.Name = "flowPanelDevices";
            this.flowPanelDevices.Padding = new System.Windows.Forms.Padding(5);
            this.flowPanelDevices.Size = new System.Drawing.Size(600, 50);
            this.flowPanelDevices.TabIndex = 0;
            this.flowPanelDevices.WrapContents = false;
            // 
            // DeviceStatusBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panelMain);
            this.Name = "DeviceStatusBar";
            this.Size = new System.Drawing.Size(800, 50);
            this.panelMain.ResumeLayout(false);
            this.flowPanelBatchOps.ResumeLayout(false);
            this.flowPanelDevices.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.FlowLayoutPanel flowPanelDevices;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBatchOps;
        private AntdUI.Button btnStartAll;
        private AntdUI.Button btnStopAll;
    }
}
