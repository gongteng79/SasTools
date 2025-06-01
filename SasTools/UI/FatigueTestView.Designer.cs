namespace SasTools.UI
{
    partial class FatigueTestView
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button1 = new AntdUI.Button();
            this.button3 = new AntdUI.Button();
            this.lbRotationTimes = new AntdUI.Label();
            this.lbStartupInterval = new AntdUI.Label();
            this.lbRotationInterval = new AntdUI.Label();
            this.lbReverseDelay = new AntdUI.Label();
            this.lbForwardDelay = new AntdUI.Label();
            this.txtForwardDelay = new AntdUI.InputNumber();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new AntdUI.Button();
            this.txtReverseDelay = new AntdUI.InputNumber();
            this.txtStartupInterval = new AntdUI.InputNumber();
            this.txtRotationTimes = new AntdUI.InputNumber();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.txtRotationInterval = new AntdUI.InputNumber();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 14.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1415, 1085);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Font = new System.Drawing.Font("微软雅黑", 14.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage1.Location = new System.Drawing.Point(4, 52);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1407, 1029);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "报警显示";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Font = new System.Drawing.Font("微软雅黑", 14.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage2.Location = new System.Drawing.Point(4, 52);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1407, 1029);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "参数设置";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button1.Location = new System.Drawing.Point(37, 9);
            this.button1.Margin = new System.Windows.Forms.Padding(37, 9, 9, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(213, 74);
            this.button1.TabIndex = 0;
            this.button1.Text = "保存";
            this.button1.Type = AntdUI.TTypeMini.Primary;
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button3.Location = new System.Drawing.Point(555, 9);
            this.button3.Margin = new System.Windows.Forms.Padding(37, 9, 9, 9);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(213, 74);
            this.button3.TabIndex = 2;
            this.button3.Text = "电批反转";
            this.button3.Type = AntdUI.TTypeMini.Primary;
            // 
            // lbRotationTimes
            // 
            this.lbRotationTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRotationTimes.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbRotationTimes.Location = new System.Drawing.Point(1114, 5);
            this.lbRotationTimes.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.lbRotationTimes.Name = "lbRotationTimes";
            this.lbRotationTimes.Size = new System.Drawing.Size(269, 75);
            this.lbRotationTimes.TabIndex = 5;
            this.lbRotationTimes.Text = "超时设置";
            this.lbRotationTimes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStartupInterval
            // 
            this.lbStartupInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStartupInterval.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbStartupInterval.Location = new System.Drawing.Point(837, 5);
            this.lbStartupInterval.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.lbStartupInterval.Name = "lbStartupInterval";
            this.lbStartupInterval.Size = new System.Drawing.Size(265, 75);
            this.lbStartupInterval.TabIndex = 4;
            this.lbStartupInterval.Text = "循环启动间隔";
            this.lbStartupInterval.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbRotationInterval
            // 
            this.lbRotationInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRotationInterval.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbRotationInterval.Location = new System.Drawing.Point(560, 5);
            this.lbRotationInterval.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.lbRotationInterval.Name = "lbRotationInterval";
            this.lbRotationInterval.Size = new System.Drawing.Size(265, 75);
            this.lbRotationInterval.TabIndex = 3;
            this.lbRotationInterval.Text = "正反转切换间隔";
            this.lbRotationInterval.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbReverseDelay
            // 
            this.lbReverseDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbReverseDelay.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbReverseDelay.Location = new System.Drawing.Point(283, 5);
            this.lbReverseDelay.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.lbReverseDelay.Name = "lbReverseDelay";
            this.lbReverseDelay.Size = new System.Drawing.Size(265, 75);
            this.lbReverseDelay.TabIndex = 2;
            this.lbReverseDelay.Text = "反转启动延时";
            this.lbReverseDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbForwardDelay
            // 
            this.lbForwardDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbForwardDelay.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbForwardDelay.Location = new System.Drawing.Point(6, 5);
            this.lbForwardDelay.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.lbForwardDelay.Name = "lbForwardDelay";
            this.lbForwardDelay.Size = new System.Drawing.Size(265, 75);
            this.lbForwardDelay.TabIndex = 0;
            this.lbForwardDelay.Text = "正转启动延时";
            this.lbForwardDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtForwardDelay
            // 
            this.txtForwardDelay.DecimalPlaces = 1;
            this.txtForwardDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtForwardDelay.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtForwardDelay.Location = new System.Drawing.Point(6, 90);
            this.txtForwardDelay.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtForwardDelay.Name = "txtForwardDelay";
            this.txtForwardDelay.Size = new System.Drawing.Size(265, 75);
            this.txtForwardDelay.TabIndex = 22;
            this.txtForwardDelay.Text = "0.0";
            this.txtForwardDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Controls.Add(this.button3);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 874);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1389, 144);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button2.Location = new System.Drawing.Point(296, 9);
            this.button2.Margin = new System.Windows.Forms.Padding(37, 9, 9, 9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(213, 74);
            this.button2.TabIndex = 1;
            this.button2.Text = "电批正转";
            this.button2.Type = AntdUI.TTypeMini.Primary;
            // 
            // txtReverseDelay
            // 
            this.txtReverseDelay.DecimalPlaces = 1;
            this.txtReverseDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReverseDelay.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtReverseDelay.Location = new System.Drawing.Point(283, 90);
            this.txtReverseDelay.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtReverseDelay.Name = "txtReverseDelay";
            this.txtReverseDelay.Size = new System.Drawing.Size(265, 75);
            this.txtReverseDelay.TabIndex = 19;
            this.txtReverseDelay.Text = "0.0";
            this.txtReverseDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtStartupInterval
            // 
            this.txtStartupInterval.DecimalPlaces = 1;
            this.txtStartupInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStartupInterval.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtStartupInterval.Location = new System.Drawing.Point(837, 90);
            this.txtStartupInterval.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtStartupInterval.Name = "txtStartupInterval";
            this.txtStartupInterval.Size = new System.Drawing.Size(265, 75);
            this.txtStartupInterval.TabIndex = 21;
            this.txtStartupInterval.Text = "0.0";
            this.txtStartupInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtRotationTimes
            // 
            this.txtRotationTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRotationTimes.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtRotationTimes.Location = new System.Drawing.Point(1114, 90);
            this.txtRotationTimes.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtRotationTimes.Name = "txtRotationTimes";
            this.txtRotationTimes.Size = new System.Drawing.Size(269, 75);
            this.txtRotationTimes.TabIndex = 23;
            this.txtRotationTimes.Text = "0";
            this.txtRotationTimes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel2.Controls.Add(this.lbRotationTimes, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbStartupInterval, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbRotationInterval, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbReverseDelay, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbForwardDelay, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtForwardDelay, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtReverseDelay, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtRotationInterval, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtStartupInterval, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtRotationTimes, 4, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 5);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 10;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1389, 859);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // txtRotationInterval
            // 
            this.txtRotationInterval.DecimalPlaces = 1;
            this.txtRotationInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRotationInterval.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtRotationInterval.Location = new System.Drawing.Point(560, 90);
            this.txtRotationInterval.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtRotationInterval.Name = "txtRotationInterval";
            this.txtRotationInterval.Size = new System.Drawing.Size(265, 75);
            this.txtRotationInterval.TabIndex = 20;
            this.txtRotationInterval.Text = "0.0";
            this.txtRotationInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1401, 1023);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // FatigueTestView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FatigueTestView";
            this.Size = new System.Drawing.Size(1415, 1085);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private AntdUI.Label lbRotationTimes;
        private AntdUI.Label lbStartupInterval;
        private AntdUI.Label lbRotationInterval;
        private AntdUI.Label lbReverseDelay;
        private AntdUI.Label lbForwardDelay;
        private AntdUI.InputNumber txtForwardDelay;
        private AntdUI.InputNumber txtReverseDelay;
        private AntdUI.InputNumber txtRotationInterval;
        private AntdUI.InputNumber txtStartupInterval;
        private AntdUI.InputNumber txtRotationTimes;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private AntdUI.Button button1;
        private AntdUI.Button button2;
        private AntdUI.Button button3;
    }
}
