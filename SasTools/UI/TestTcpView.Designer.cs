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
            this.functionComboBox = new System.Windows.Forms.ComboBox();
            this.button1 = new AntdUI.Button();
            this.label1 = new AntdUI.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnConnect = new AntdUI.Button();
            this.btnDisConnect = new AntdUI.Button();
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
            this.splitter1.Size = new System.Drawing.Size(719, 494);
            this.splitter1.SplitterDistance = 237;
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
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(719, 237);
            this.panel1.TabIndex = 0;
            // 
            // functionComboBox
            // 
            this.functionComboBox.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.functionComboBox.FormattingEnabled = true;
            this.functionComboBox.Location = new System.Drawing.Point(36, 61);
            this.functionComboBox.Name = "functionComboBox";
            this.functionComboBox.Size = new System.Drawing.Size(121, 27);
            this.functionComboBox.TabIndex = 3;
            this.functionComboBox.SelectedIndexChanged += new System.EventHandler(this.functionComboBox_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(36, 119);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 32);
            this.button1.TabIndex = 2;
            this.button1.Text = "发送";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(36, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择指令";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(719, 253);
            this.textBox1.TabIndex = 0;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(319, 23);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(84, 32);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "设备连接";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(435, 23);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(84, 32);
            this.btnDisConnect.TabIndex = 5;
            this.btnDisConnect.Text = "断开连接";
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // TestTcpView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitter1);
            this.Name = "TestTcpView";
            this.Size = new System.Drawing.Size(719, 494);
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
