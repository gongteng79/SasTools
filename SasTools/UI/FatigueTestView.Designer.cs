using SasTools.Events;
using System;

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
            if (disposing)
            {
                try
                {
                    // 取消所有后台任务
                    if (_backgroundTasksCancellationTokenSource != null)
                    {
                        _backgroundTasksCancellationTokenSource.Cancel();
                        _backgroundTasksCancellationTokenSource.Dispose();
                        _backgroundTasksCancellationTokenSource = null;
                    }


                    // 取消所有EventBus事件订阅
                    if (_eventBus != null)
                    {
                        _eventBus.Unsubscribe<RefreshMachineState>(this);
                        _eventBus.Unsubscribe<CounterUpdateEvent>(this);
                        _eventBus.Unsubscribe<MultiDeviceCreateEvent>(this);
                        _eventBus.Unsubscribe<MultiDeviceRemoveEvent>(this);
                    }

                    // 取消DeviceManager事件订阅
                    if (_deviceManager != null)
                    {
                        _deviceManager.DeviceSelectionChanged -= OnDeviceManagerSelectionChanged;
                        _deviceManager.DeviceStatusChanged -= OnDeviceStatusChanged;
                    }

                    // 取消DeviceStatusBar事件订阅
                    if (deviceStatusBar1 != null)
                    {
                        deviceStatusBar1.DeviceSelected -= OnDeviceStatusBarSelected;
                        deviceStatusBar1.StartAllClicked -= OnStartAllClicked;
                        deviceStatusBar1.StopAllClicked -= OnStopAllClicked;
                    }

                    // 停止所有设备测试并清理资源
                    if (_deviceTables != null)
                    {
                        foreach (var deviceTable in _deviceTables.Values)
                        {
                            try
                            {
                                // 停止设备测试
                                var stateMachine = _deviceManager?.GetDeviceStateMachine(deviceTable.DeviceId);
                                if (stateMachine != null)
                                {
                                    stateMachine.StopTest();
                                    stateMachine.Dispose();
                                }

                                // 释放设备表格资源
                                deviceTable.Dispose();
                            }
                            catch (Exception ex)
                            {
                                _logger?.Error($"释放设备 {deviceTable.DeviceId} 资源失败: {ex.Message}", ex);
                            }
                        }
                        _deviceTables.Clear();
                        _deviceTables = null;
                    }

                    // 清理其他资源
                    _smartDataCleanup = null;
                    _runningDevices?.Clear();
                    _runningDevices = null;
                    lastCleanupTime?.Clear();
                    lastCleanupTime = null;
                    // 清理UI更新批处理器
                    _uiUpdateBatcher?.Dispose();
                    _uiUpdateBatcher = null;
                    // 清理DataTable
                    _dataTable?.Clear();
                    _dataTable?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.Error($"释放FatigueTestView资源时发生异常: {ex.Message}", ex);
                }

                if (components != null)
                {
                    components.Dispose();
                }
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
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.deviceStatusBar1 = new SasTools.UI.DeviceStatusBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.divider1 = new AntdUI.Divider();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.button5 = new AntdUI.Button();
            this.button6 = new AntdUI.Button();
            this.button7 = new AntdUI.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.badge1 = new AntdUI.Badge();
            this.divider2 = new AntdUI.Divider();
            this.panel5 = new System.Windows.Forms.Panel();
            this.input3 = new AntdUI.Input();
            this.divider5 = new AntdUI.Divider();
            this.panel7 = new System.Windows.Forms.Panel();
            this.input2 = new AntdUI.Input();
            this.divider4 = new AntdUI.Divider();
            this.panel6 = new System.Windows.Forms.Panel();
            this.input1 = new AntdUI.Input();
            this.divider3 = new AntdUI.Divider();
            this.panelTableContainer = new AntdUI.Panel();
            this.panelCurrentTable = new AntdUI.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.inputNumber1 = new AntdUI.InputNumber();
            this.label1 = new AntdUI.Label();
            this.lbReverseDelay = new AntdUI.Label();
            this.lbForwardDelay = new AntdUI.Label();
            this.txtForwardDelay = new AntdUI.InputNumber();
            this.txtReverseDelay = new AntdUI.InputNumber();
            this.lbRotationTimes = new AntdUI.Label();
            this.txtRotationTimes = new AntdUI.InputNumber();
            this.label2 = new AntdUI.Label();
            this.lbRotationInterval = new AntdUI.Label();
            this.inputNumber2 = new AntdUI.InputNumber();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new AntdUI.Button();
            this.button2 = new AntdUI.Button();
            this.button3 = new AntdUI.Button();
            this.label3 = new AntdUI.Label();
            this.label4 = new AntdUI.Label();
            this.txtReserveTime = new AntdUI.InputNumber();
            this.txtReserveSpeed = new AntdUI.InputNumber();
            this.tableLayoutPanelMain.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panelTableContainer.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 2;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(1158, 930);
            this.tableLayoutPanelMain.TabIndex = 2;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.deviceStatusBar1);
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1152, 54);
            this.panel4.TabIndex = 2;
            // 
            // deviceStatusBar1
            // 
            this.deviceStatusBar1.BackColor = System.Drawing.Color.White;
            this.deviceStatusBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceStatusBar1.Location = new System.Drawing.Point(0, 0);
            this.deviceStatusBar1.Name = "deviceStatusBar1";
            this.deviceStatusBar1.Size = new System.Drawing.Size(1152, 54);
            this.deviceStatusBar1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 14.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(3, 63);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1152, 864);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel3);
            this.tabPage1.Font = new System.Drawing.Font("微软雅黑", 14.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage1.Location = new System.Drawing.Point(4, 45);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1144, 815);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "报警显示";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.Controls.Add(this.divider1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.panel1, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel5, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.panelTableContainer, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1138, 809);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // divider1
            // 
            this.divider1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.divider1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.divider1.Location = new System.Drawing.Point(3, 3);
            this.divider1.Name = "divider1";
            this.divider1.Size = new System.Drawing.Size(69, 728);
            this.divider1.TabIndex = 0;
            this.divider1.Text = "报警显示";
            this.divider1.Vertical = true;
            // 
            // flowLayoutPanel2
            // 
            this.tableLayoutPanel3.SetColumnSpan(this.flowLayoutPanel2, 4);
            this.flowLayoutPanel2.Controls.Add(this.button5);
            this.flowLayoutPanel2.Controls.Add(this.button6);
            this.flowLayoutPanel2.Controls.Add(this.button7);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 737);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(1132, 69);
            this.flowLayoutPanel2.TabIndex = 2;
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button5.Location = new System.Drawing.Point(3, 3);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(174, 63);
            this.button5.TabIndex = 2;
            this.button5.Text = "启动测试";
            this.button5.Type = AntdUI.TTypeMini.Primary;
            this.button5.Click += new System.EventHandler(this.Button5_Click);
            // 
            // button6
            // 
            this.button6.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button6.Location = new System.Drawing.Point(183, 3);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(174, 63);
            this.button6.TabIndex = 3;
            this.button6.Text = "停止测试";
            this.button6.Type = AntdUI.TTypeMini.Primary;
            this.button6.Click += new System.EventHandler(this.Button6_Click);
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button7.Location = new System.Drawing.Point(363, 3);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(174, 63);
            this.button7.TabIndex = 4;
            this.button7.Text = "复位";
            this.button7.Type = AntdUI.TTypeMini.Primary;
            this.button7.Click += new System.EventHandler(this.Button7_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.divider2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(715, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(206, 728);
            this.panel1.TabIndex = 3;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.badge1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 66);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(206, 75);
            this.panel2.TabIndex = 1;
            // 
            // badge1
            // 
            this.badge1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.badge1.DotRatio = 0.6F;
            this.badge1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.badge1.Location = new System.Drawing.Point(0, 0);
            this.badge1.Name = "badge1";
            this.badge1.Size = new System.Drawing.Size(206, 75);
            this.badge1.State = AntdUI.TState.Success;
            this.badge1.TabIndex = 0;
            this.badge1.Text = "运行状态";
            this.badge1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // divider2
            // 
            this.divider2.Dock = System.Windows.Forms.DockStyle.Top;
            this.divider2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.divider2.Location = new System.Drawing.Point(0, 0);
            this.divider2.Name = "divider2";
            this.divider2.Size = new System.Drawing.Size(206, 66);
            this.divider2.TabIndex = 0;
            this.divider2.Text = "状态显示";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.input3);
            this.panel5.Controls.Add(this.divider5);
            this.panel5.Controls.Add(this.panel7);
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(927, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(208, 728);
            this.panel5.TabIndex = 4;
            // 
            // input3
            // 
            this.input3.Dock = System.Windows.Forms.DockStyle.Top;
            this.input3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.input3.Location = new System.Drawing.Point(0, 242);
            this.input3.Name = "input3";
            this.input3.ReadOnly = true;
            this.input3.Size = new System.Drawing.Size(208, 52);
            this.input3.TabIndex = 3;
            this.input3.Text = "0";
            this.input3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // divider5
            // 
            this.divider5.Dock = System.Windows.Forms.DockStyle.Top;
            this.divider5.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.divider5.Location = new System.Drawing.Point(0, 196);
            this.divider5.Name = "divider5";
            this.divider5.Size = new System.Drawing.Size(208, 46);
            this.divider5.TabIndex = 2;
            this.divider5.Text = "失败次数";
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.input2);
            this.panel7.Controls.Add(this.divider4);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(0, 98);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(208, 98);
            this.panel7.TabIndex = 1;
            // 
            // input2
            // 
            this.input2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.input2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.input2.Location = new System.Drawing.Point(0, 46);
            this.input2.Name = "input2";
            this.input2.ReadOnly = true;
            this.input2.Size = new System.Drawing.Size(208, 52);
            this.input2.TabIndex = 2;
            this.input2.Text = "0";
            this.input2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // divider4
            // 
            this.divider4.Dock = System.Windows.Forms.DockStyle.Top;
            this.divider4.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.divider4.Location = new System.Drawing.Point(0, 0);
            this.divider4.Name = "divider4";
            this.divider4.Size = new System.Drawing.Size(208, 46);
            this.divider4.TabIndex = 1;
            this.divider4.Text = "成功次数";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.input1);
            this.panel6.Controls.Add(this.divider3);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(208, 98);
            this.panel6.TabIndex = 0;
            // 
            // input1
            // 
            this.input1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.input1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.input1.Location = new System.Drawing.Point(0, 46);
            this.input1.Name = "input1";
            this.input1.ReadOnly = true;
            this.input1.Size = new System.Drawing.Size(208, 52);
            this.input1.TabIndex = 2;
            this.input1.Text = "0";
            this.input1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // divider3
            // 
            this.divider3.Dock = System.Windows.Forms.DockStyle.Top;
            this.divider3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.divider3.Location = new System.Drawing.Point(0, 0);
            this.divider3.Name = "divider3";
            this.divider3.Size = new System.Drawing.Size(208, 46);
            this.divider3.TabIndex = 1;
            this.divider3.Text = "总循环次数";
            // 
            // panelTableContainer
            // 
            this.panelTableContainer.BackColor = System.Drawing.Color.LightGray;
            this.panelTableContainer.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(119)))), ((int)(((byte)(255)))));
            this.panelTableContainer.Controls.Add(this.panelCurrentTable);
            this.panelTableContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTableContainer.Location = new System.Drawing.Point(78, 3);
            this.panelTableContainer.Name = "panelTableContainer";
            this.panelTableContainer.Size = new System.Drawing.Size(631, 728);
            this.panelTableContainer.TabIndex = 5;
            this.panelTableContainer.Text = "panel3";
            // 
            // panelCurrentTable
            // 
            this.panelCurrentTable.BackColor = System.Drawing.Color.White;
            this.panelCurrentTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCurrentTable.Location = new System.Drawing.Point(0, 0);
            this.panelCurrentTable.Name = "panelCurrentTable";
            this.panelCurrentTable.Size = new System.Drawing.Size(631, 728);
            this.panelCurrentTable.TabIndex = 0;
            this.panelCurrentTable.Text = "panel3";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel1);
            this.tabPage2.Font = new System.Drawing.Font("微软雅黑", 14.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage2.Location = new System.Drawing.Point(4, 45);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1144, 815);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "参数设置";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1138, 809);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Controls.Add(this.inputNumber1, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbReverseDelay, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbForwardDelay, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtForwardDelay, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtReverseDelay, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.lbRotationTimes, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtRotationTimes, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.label2, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.inputNumber2, 4, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lbRotationInterval, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.label4, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.txtReserveTime, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.txtReserveSpeed, 1, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1128, 679);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // inputNumber1
            // 
            this.inputNumber1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputNumber1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.inputNumber1.Location = new System.Drawing.Point(680, 71);
            this.inputNumber1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.inputNumber1.Name = "inputNumber1";
            this.inputNumber1.Size = new System.Drawing.Size(215, 59);
            this.inputNumber1.TabIndex = 25;
            this.inputNumber1.Text = "0";
            this.inputNumber1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(680, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(215, 59);
            this.label1.TabIndex = 24;
            this.label1.Text = "循环次数";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbReverseDelay
            // 
            this.lbReverseDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbReverseDelay.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbReverseDelay.Location = new System.Drawing.Point(230, 4);
            this.lbReverseDelay.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.lbReverseDelay.Name = "lbReverseDelay";
            this.lbReverseDelay.Size = new System.Drawing.Size(215, 59);
            this.lbReverseDelay.TabIndex = 2;
            this.lbReverseDelay.Text = "反转启动延时";
            this.lbReverseDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbForwardDelay
            // 
            this.lbForwardDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbForwardDelay.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbForwardDelay.Location = new System.Drawing.Point(5, 4);
            this.lbForwardDelay.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.lbForwardDelay.Name = "lbForwardDelay";
            this.lbForwardDelay.Size = new System.Drawing.Size(215, 59);
            this.lbForwardDelay.TabIndex = 0;
            this.lbForwardDelay.Text = "正转启动延时";
            this.lbForwardDelay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtForwardDelay
            // 
            this.txtForwardDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtForwardDelay.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtForwardDelay.Location = new System.Drawing.Point(5, 71);
            this.txtForwardDelay.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtForwardDelay.Name = "txtForwardDelay";
            this.txtForwardDelay.Size = new System.Drawing.Size(215, 59);
            this.txtForwardDelay.TabIndex = 22;
            this.txtForwardDelay.Text = "0";
            this.txtForwardDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtReverseDelay
            // 
            this.txtReverseDelay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReverseDelay.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtReverseDelay.Location = new System.Drawing.Point(230, 71);
            this.txtReverseDelay.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtReverseDelay.Name = "txtReverseDelay";
            this.txtReverseDelay.Size = new System.Drawing.Size(215, 59);
            this.txtReverseDelay.TabIndex = 19;
            this.txtReverseDelay.Text = "0";
            this.txtReverseDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lbRotationTimes
            // 
            this.lbRotationTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRotationTimes.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbRotationTimes.Location = new System.Drawing.Point(455, 4);
            this.lbRotationTimes.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.lbRotationTimes.Name = "lbRotationTimes";
            this.lbRotationTimes.Size = new System.Drawing.Size(215, 59);
            this.lbRotationTimes.TabIndex = 5;
            this.lbRotationTimes.Text = "超时设置";
            this.lbRotationTimes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtRotationTimes
            // 
            this.txtRotationTimes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRotationTimes.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtRotationTimes.Location = new System.Drawing.Point(455, 71);
            this.txtRotationTimes.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtRotationTimes.Name = "txtRotationTimes";
            this.txtRotationTimes.Size = new System.Drawing.Size(215, 59);
            this.txtRotationTimes.TabIndex = 23;
            this.txtRotationTimes.Text = "0";
            this.txtRotationTimes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(905, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 59);
            this.label2.TabIndex = 26;
            this.label2.Text = "NG次数";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbRotationInterval
            // 
            this.lbRotationInterval.Location = new System.Drawing.Point(453, 137);
            this.lbRotationInterval.Name = "lbRotationInterval";
            this.lbRotationInterval.Size = new System.Drawing.Size(0, 0);
            this.lbRotationInterval.TabIndex = 6;
            // 
            // inputNumber2
            // 
            this.inputNumber2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputNumber2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.inputNumber2.Location = new System.Drawing.Point(905, 71);
            this.inputNumber2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.inputNumber2.Name = "inputNumber2";
            this.inputNumber2.Size = new System.Drawing.Size(218, 59);
            this.inputNumber2.TabIndex = 27;
            this.inputNumber2.Text = "0";
            this.inputNumber2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnSave);
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Controls.Add(this.button3);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 691);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1128, 114);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnSave.Location = new System.Drawing.Point(30, 8);
            this.btnSave.Margin = new System.Windows.Forms.Padding(30, 8, 7, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(174, 63);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存";
            this.btnSave.Type = AntdUI.TTypeMini.Primary;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button2.Location = new System.Drawing.Point(241, 8);
            this.button2.Margin = new System.Windows.Forms.Padding(30, 8, 7, 8);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(174, 63);
            this.button2.TabIndex = 1;
            this.button2.Text = "电批正转";
            this.button2.Type = AntdUI.TTypeMini.Primary;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.button3.Location = new System.Drawing.Point(452, 8);
            this.button3.Margin = new System.Windows.Forms.Padding(30, 8, 7, 8);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(174, 63);
            this.button3.TabIndex = 2;
            this.button3.Text = "电批反转";
            this.button3.Type = AntdUI.TTypeMini.Primary;
            this.button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(5, 138);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(215, 59);
            this.label3.TabIndex = 28;
            this.label3.Text = "反转时间";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(230, 138);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(215, 59);
            this.label4.TabIndex = 29;
            this.label4.Text = "反转速度";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtReserveTime
            // 
            this.txtReserveTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReserveTime.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtReserveTime.Location = new System.Drawing.Point(5, 205);
            this.txtReserveTime.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtReserveTime.Name = "txtReserveTime";
            this.txtReserveTime.Size = new System.Drawing.Size(215, 59);
            this.txtReserveTime.TabIndex = 30;
            this.txtReserveTime.Text = "0";
            this.txtReserveTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtReserveSpeed
            // 
            this.txtReserveSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReserveSpeed.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.txtReserveSpeed.Location = new System.Drawing.Point(230, 205);
            this.txtReserveSpeed.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.txtReserveSpeed.Name = "txtReserveSpeed";
            this.txtReserveSpeed.Size = new System.Drawing.Size(215, 59);
            this.txtReserveSpeed.TabIndex = 31;
            this.txtReserveSpeed.Text = "0";
            this.txtReserveSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FatigueTestView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "FatigueTestView";
            this.Size = new System.Drawing.Size(1158, 930);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panelTableContainer.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel panel4;
        private DeviceStatusBar deviceStatusBar1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panel5;
        private AntdUI.Input input3;
        private AntdUI.Divider divider5;
        private System.Windows.Forms.Panel panel7;
        private AntdUI.Input input2;
        private AntdUI.Divider divider4;
        private System.Windows.Forms.Panel panel6;
        private AntdUI.Input input1;
        private AntdUI.Divider divider3;
        private AntdUI.Divider divider1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private AntdUI.Button button5;
        private AntdUI.Button button6;
        private AntdUI.Button button7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private AntdUI.Badge badge1;
        private AntdUI.Divider divider2;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private AntdUI.InputNumber inputNumber1;
        private AntdUI.Label label1;
        private AntdUI.Label lbReverseDelay;
        private AntdUI.Label lbForwardDelay;
        private AntdUI.InputNumber txtForwardDelay;
        private AntdUI.InputNumber txtReverseDelay;
        private AntdUI.Label lbRotationTimes;
        private AntdUI.InputNumber txtRotationTimes;
        private AntdUI.Label label2;
        private AntdUI.Label lbRotationInterval;
        private AntdUI.InputNumber inputNumber2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private AntdUI.Button btnSave;
        private AntdUI.Button button2;
        private AntdUI.Button button3;
        private AntdUI.Panel panelTableContainer;
        private AntdUI.Panel panelCurrentTable;
        private AntdUI.InputNumber txtReserveSpeed;
        private AntdUI.Label label3;
        private AntdUI.Label label4;
        private AntdUI.InputNumber txtReserveTime;
    }
}
