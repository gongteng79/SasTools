using AntdUI;
using System.Drawing;
using System.Windows.Forms;

namespace SasTools
{
    partial class OverView
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            AntdUI.MenuItem menuItem1 = new AntdUI.MenuItem();
            AntdUI.MenuItem menuItem2 = new AntdUI.MenuItem();
            AntdUI.MenuItem menuItem3 = new AntdUI.MenuItem();
            this.windowBar = new AntdUI.PageHeader();
            this.button1 = new AntdUI.Button();
            this.btnLogin = new AntdUI.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menu1 = new AntdUI.Menu();
            this.pnlView = new System.Windows.Forms.Panel();
            this.windowBar.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // windowBar
            // 
            this.windowBar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.windowBar.Controls.Add(this.button1);
            this.windowBar.Controls.Add(this.btnLogin);
            this.windowBar.DividerMargin = 3;
            this.windowBar.DividerShow = true;
            this.windowBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.windowBar.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.windowBar.Location = new System.Drawing.Point(0, 0);
            this.windowBar.Name = "windowBar";
            this.windowBar.ShowButton = true;
            this.windowBar.Size = new System.Drawing.Size(1300, 58);
            this.windowBar.SubText = "Version 1.0.0.1";
            this.windowBar.TabIndex = 0;
            this.windowBar.Text = "TestTool";
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.IconSvg = "PlusOutlined";
            this.button1.Location = new System.Drawing.Point(706, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(153, 58);
            this.button1.TabIndex = 1;
            this.button1.Text = "连接设备";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLogin.IconSvg = "UserSwitchOutlined";
            this.btnLogin.Location = new System.Drawing.Point(859, 0);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(153, 58);
            this.btnLogin.TabIndex = 0;
            this.btnLogin.Text = "用户登录";
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.menu1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnlView, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 58);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1300, 662);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // menu1
            // 
            this.menu1.Dock = System.Windows.Forms.DockStyle.Fill;
            menuItem1.IconSvg = "HomeOutlined";
            menuItem1.Text = "主界面";
            menuItem2.IconSvg = "SettingOutlined";
            menuItem2.Text = "参数配置";
            menuItem3.IconSvg = "FileTextOutlined";
            menuItem3.Text = "配方管理";
            this.menu1.Items.Add(menuItem1);
            this.menu1.Items.Add(menuItem2);
            this.menu1.Items.Add(menuItem3);
            this.menu1.Location = new System.Drawing.Point(4, 4);
            this.menu1.Name = "menu1";
            this.menu1.Size = new System.Drawing.Size(194, 654);
            this.menu1.TabIndex = 1;
            this.menu1.Text = "menu1";
            this.menu1.SelectChanged += new AntdUI.SelectEventHandler(this.menu1_SelectChanged);
            // 
            // pnlView
            // 
            this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlView.Location = new System.Drawing.Point(205, 4);
            this.pnlView.Name = "pnlView";
            this.pnlView.Size = new System.Drawing.Size(1091, 654);
            this.pnlView.TabIndex = 2;
            // 
            // OverView
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1300, 720);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.windowBar);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.ForeColor = System.Drawing.Color.Black;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MinimumSize = new System.Drawing.Size(660, 400);
            this.Name = "OverView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AntdUI Overview";
            this.windowBar.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PageHeader windowBar;
        private TableLayoutPanel tableLayoutPanel1;
        private AntdUI.Menu menu1;
        private AntdUI.Button btnLogin;
        private AntdUI.Button button1;
        private System.Windows.Forms.Panel pnlView;
    }
}

