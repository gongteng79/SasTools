namespace SasTools.UI
{
    partial class DeviceManagementView
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelContent;
        private AntdUI.Divider dividerTitle;

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
            this.panelContent = new System.Windows.Forms.Panel();
            this.dividerTitle = new AntdUI.Divider();
            this.SuspendLayout();

            // 
            // dividerTitle
            // 
            this.dividerTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.dividerTitle.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.dividerTitle.Location = new System.Drawing.Point(0, 0);
            this.dividerTitle.Name = "dividerTitle";
            this.dividerTitle.Size = new System.Drawing.Size(800, 50);
            this.dividerTitle.TabIndex = 0;
            this.dividerTitle.Text = "设备管理";

            // 
            // panelContent
            // 
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 50);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(800, 550);
            this.panelContent.TabIndex = 1;

            // 
            // DeviceManagementView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.dividerTitle);
            this.Name = "DeviceManagementView";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
        }
    }
}
