namespace SasTools.UI
{
    partial class TestTcpView
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
            this.splitter1 = new AntdUI.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDisConnect = new AntdUI.Button();
            this.btnConnect = new AntdUI.Button();
            this.functionComboBox = new System.Windows.Forms.ComboBox();
            this.button1 = new AntdUI.Button();
            this.label1 = new AntdUI.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitter1)).BeginInit();
            this.splitter1.Panel1.SuspendLayout();
            this.splitter1.Panel2.SuspendLayout();
            this.splitter1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.splitter1.Name = "splitter1";
            this.splitter1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitter1.Panel1
            // 
            this.splitter1.Panel1.Controls.Add(this.panel1);
            // 
            // splitter1.Panel2
            // 
            this.splitter1.Panel2.Controls.Add(this.textBox1);
            this.splitter1.Size = new System.Drawing.Size(1318, 864);
            this.splitter1.SplitterDistance = 414;
            this.splitter1.SplitterWidth = 7;
            this.splitter1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnDisConnect);
            this.panel1.Controls.Add(this.btnConnect);
            this.panel1.Controls.Add(this.functionComboBox);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1318, 414);
            this.panel1.TabIndex = 0;
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(798, 40);
            this.btnDisConnect.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(154, 56);
            this.btnDisConnect.TabIndex = 5;
            this.btnDisConnect.Text = "断开连接";
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(585, 40);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(154, 56);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "设备连接";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // functionComboBox
            // 
            this.functionComboBox.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.functionComboBox.FormattingEnabled = true;
            this.functionComboBox.Location = new System.Drawing.Point(66, 107);
            this.functionComboBox.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.functionComboBox.Name = "functionComboBox";
            this.functionComboBox.Size = new System.Drawing.Size(218, 41);
            this.functionComboBox.TabIndex = 3;
            this.functionComboBox.SelectedIndexChanged += new System.EventHandler(this.functionComboBox_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(66, 218);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(154, 56);
            this.button1.TabIndex = 2;
            this.button1.Text = "发送";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(66, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 56);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择指令";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(1318, 443);
            this.textBox1.TabIndex = 0;
            // 
            // TestTcpView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitter1);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "TestTcpView";
            this.Size = new System.Drawing.Size(1318, 864);
            this.splitter1.Panel1.ResumeLayout(false);
            this.splitter1.Panel2.ResumeLayout(false);
            this.splitter1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitter1)).EndInit();
            this.splitter1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private AntdUI.Splitter splitter1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel1;
        private AntdUI.Label label1;
        private AntdUI.Button button1;
        private System.Windows.Forms.ComboBox functionComboBox;
        private AntdUI.Button btnConnect;
        private AntdUI.Button btnDisConnect;
    }
}
