using log4net;
using SasTools.Domain;
using SasTools.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;
using System.Runtime.CompilerServices;

namespace SasTools.UI
{
    public partial class AbnormalAlarmView : UserControl
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AbnormalAlarmView));
        private ITestStateMachine _stateMachine;
        private ISasTest _sasTest;
        private IParameterService _parameterService;

        private DataTable dataTable;

        public AbnormalAlarmView(ISasTest sasTest, IParameterService parameterService)
        {
            InitializeComponent();

            // 初始化表格
            InitializeTable();

            // 初始化按钮事件
            InitializeButtons();

            // 初始化状态显示
            InitializeStatusDisplay();

            Initialize(sasTest, parameterService);
        }

        // 由主窗体调用，传入依赖服务
        public void Initialize(ISasTest sasTest, IParameterService parameterService)
        {
            _sasTest = sasTest;
            _parameterService = parameterService;

            // 创建状态机
            _stateMachine = new TestStateMachine(_sasTest, _parameterService);
            _stateMachine.StateChanged += StateMachine_StateChanged;
            _stateMachine.TestResultReceived += StateMachine_TestResultReceived;
            //_stateMachine.CounterChanged += AbnormalAlarmView_CounterChanged;

            // 更新UI状态
            UpdateControlState(false);

            // 添加日志信息
            AddLogMessage("系统就绪");
        }

        private void AbnormalAlarmView_CounterChanged(object sender, TestCounterEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                input1.Text = e.TotalCycles.ToString();  // 总循环次数
                input2.Text = e.SuccessCount.ToString(); // 成功次数

                // 更新状态标签
                badge2.Text = $"总循环: {e.TotalCycles}";
                badge3.Text = $"成功: {e.SuccessCount}";
            }));
        }

        private void InitializeTable()
        {
            // 定义列
            var columns = new ColumnCollection
            {
                new Column("Time", "时间"),
                new Column("Info", "消息"),
            };
            tableAlarmInfo.Columns = columns;

            // 绑定数据源
            dataTable = new DataTable();
            dataTable.Columns.Add("Time", typeof(string));
            dataTable.Columns.Add("Info", typeof(string));
            tableAlarmInfo.DataSource = dataTable;
            tableAlarmInfo.ColumnDragSort = true;
        }

        private void InitializeButtons()
        {
            // 参数设置按钮
            button1.Text = "参数设置";
            button1.Click += async (s, e) =>
            {
                try
                {
                    // 打开参数设置窗口逻辑
                    AntdUI.Message.info(this.ParentForm, "参数设置功能正在开发中");
                }
                catch (Exception ex)
                {
                    _logger.Error($"打开参数设置窗口失败: {ex.Message}", ex);
                    AntdUI.Message.error(this.ParentForm, "打开参数设置窗口失败");
                }
            };

            // 启动测试按钮
            button2.Text = "启动测试";
            button2.Click += async (s, e) => await StartTestAsync();

            // 停止测试按钮
            button3.Text = "停止测试";
            button3.Click += async (s, e) => await StopTestAsync();

            // 复位按钮
            button4.Text = "复位";
            button4.Click += async (s, e) => await ResetAsync();
        }

        private void InitializeStatusDisplay()
        {
            // 添加状态标签
            var lblStatus = new AntdUI.Label
            {
                Text = "当前状态: 空闲",
                AutoSize = true,
                Font = new Font("微软雅黑", 12),
                Location = new Point(10, 10)
            };
            panel1.Controls.Add(lblStatus);

            // 设置标签
            badge1.Text = "测试状态: 空闲";
            badge1.State = AntdUI.TState.Default;

            badge2.Text = "总循环: 0";
            badge2.State = AntdUI.TState.Default;

            badge3.Text = "成功: 0";
            badge3.State = AntdUI.TState.Default;

            // 设置输入框标题
            divider3.Text = "总循环次数";
            divider4.Text = "成功次数";
            divider5.Text = "设备周期";

            // 设置输入框内容
            input1.Text = "0"; // 总循环次数
            input2.Text = "0"; // 成功次数
            input3.Text = "0"; // 设备周期
        }

        // 状态机状态变更事件处理
        private void StateMachine_StateChanged(object sender, TestState state)
        {
            // 在UI线程中更新界面
            this.Invoke(new Action(() =>
            {
                AddLogMessage($"测试状态变更: {state}");

                // 更新状态标签
                var lblStatus = panel1.Controls.OfType<AntdUI.Label>().FirstOrDefault();
                if (lblStatus != null)
                {
                    lblStatus.Text = $"当前状态: {state}";
                }

                // 更新状态指示器
                UpdateStatusIndicator(state);

                // 根据状态更新UI控件
                UpdateControlState(state != TestState.Idle && state != TestState.Error);
            }));
        }

        private void UpdateStatusIndicator(TestState state)
        {
            switch (state)
            {
                case TestState.Idle:
                    badge1.Text = "测试状态: 空闲";
                    badge1.State = AntdUI.TState.Default;
                    break;
                case TestState.Initializing:
                    badge1.Text = "测试状态: 初始化";
                    badge1.State = AntdUI.TState.Processing;
                    break;
                case TestState.Forward:
                case TestState.ForwardWaiting:
                case TestState.ForwardDelay:
                    badge1.Text = "测试状态: 正转";
                    badge1.State = AntdUI.TState.Processing;
                    break;
                case TestState.Reverse:
                case TestState.ReverseWaiting:
                case TestState.ReverseDelay:
                    badge1.Text = "测试状态: 反转";
                    badge1.State = AntdUI.TState.Processing;
                    break;
                case TestState.StartupInterval:
                    badge1.Text = "测试状态: 循环间隔";
                    badge1.State = AntdUI.TState.Processing;
                    break;
                case TestState.Stopping:
                    badge1.Text = "测试状态: 停止中";
                    badge1.State = AntdUI.TState.Warn;  
                    break;
                case TestState.Error:
                    badge1.Text = "测试状态: 错误";
                    badge1.State = AntdUI.TState.Error;
                    break;
                default:
                    badge1.Text = $"测试状态: {state}";
                    badge1.State = AntdUI.TState.Processing;
                    break;
            }
        }

        // 测试结果事件处理
        private void StateMachine_TestResultReceived(object sender, TestResult result)
        {
            this.Invoke(new Action(() =>
            {
                string resultText = result.IsSuccess ? "成功" : "失败";
                AddLogMessage($"测试结果: {resultText} - {result.Message}");

                // 更新成功/失败状态指示
                if (result.Message.Contains("正转"))
                {
                    badge2.State = result.IsSuccess ? AntdUI.TState.Success : AntdUI.TState.Error;
                }
                else if (result.Message.Contains("反转"))
                {
                    badge3.State = result.IsSuccess ? AntdUI.TState.Success : AntdUI.TState.Error;
                }

                // 如果失败，显示错误消息
                if (!result.IsSuccess)
                {
                    AntdUI.Message.error(this.ParentForm, result.Message);
                }
            }));
        }

        // 启动测试
        private async Task StartTestAsync()
        {
            try
            {
                if (_stateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, "测试系统未初始化");
                    return;
                }

                button2.Loading = true;

                // 启动测试
                bool success = await _stateMachine.StartTestAsync();
                button2.Enabled = false;
                button3.Enabled = true; // 启动后允许停止测试
                //if (!success)
                //{
                //    AntdUI.Message.error(this.ParentForm, "启动测试失败");
                //}
                //else
                //{
                //    AddLogMessage("测试已启动");
                //    AntdUI.Message.success(this.ParentForm, "测试已启动");
                //}

                //button2.Loading = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"启动测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"启动测试发生错误: {ex.Message}");
                button2.Loading = false;
            }
        }

        // 停止测试
        private async Task StopTestAsync()
        {
            try
            {
                if (_stateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, "测试系统未初始化");
                    return;
                }

                button3.Loading = true;

                // 停止测试
                bool success = await _stateMachine.StopTestAsync();
                button3.Loading = false;
                button3.Enabled = false;
                button2.Loading = false;
                button2.Enabled = true;
                //if (!success)
                //{
                //    AntdUI.Message.error(this.ParentForm, "停止测试失败");
                //}
                //else
                //{
                //    AddLogMessage("测试已停止");
                //    AntdUI.Message.info(this.ParentForm, "测试已停止");
                //}

                //button3.Loading = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"停止测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"停止测试发生错误: {ex.Message}");
                button3.Loading = false;
            }
        }

        // 复位
        private async Task ResetAsync()
        {
            try
            {
                if (_stateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, "测试系统未初始化");
                    return;
                }

                button4.Loading = true;

                // 复位
                bool success = await _stateMachine.ResetAsync();

                if (!success)
                {
                    AntdUI.Message.error(this.ParentForm, "复位失败");
                }
                else
                {
                    AddLogMessage("系统已复位");
                    AntdUI.Message.success(this.ParentForm, "系统已复位");

                    // 更新状态显示
                    badge1.Text = "测试状态: 空闲";
                    badge1.State = AntdUI.TState.Default;
                    badge2.Text = "总循环: 0";
                    badge2.State = AntdUI.TState.Default;
                    badge3.Text = "成功: 0";
                    badge3.State = AntdUI.TState.Default;

                    // 更新输入框
                    input1.Text = "0";
                    input2.Text = "0";
                    input3.Text = "0";
                }

                button4.Loading = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"复位失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"复位发生错误: {ex.Message}");
                button4.Loading = false;
            }
        }

        // 更新控件状态
        private void UpdateControlState(bool isRunning)
        {
            button1.Enabled = !isRunning; // 参数设置按钮
            button2.Enabled = !isRunning; // 启动测试按钮
            button3.Enabled = isRunning;  // 停止测试按钮
            button4.Enabled = true;       // 复位按钮始终可用
        }

        // Replace the problematic code block in the AddLogMessage method with the following:

        private void AddLogMessage(string message)
        {
            DataRow row = dataTable.NewRow();
            row["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["Info"] = message;
            dataTable.Rows.Add(row);

            // 限制显示的记录数量，保持最新的100条
            if (dataTable.Rows.Count > 100)
            {
                dataTable.Rows.RemoveAt(0);
            }

            // 滚动到最新消息
            try
            {
                if (tableAlarmInfo.DataSource is DataTable dataSource && dataSource.Rows.Count > 0)
                {
                    tableAlarmInfo.SelectedIndex = dataSource.Rows.Count - 1;
                }
            }
            catch
            {
                // 忽略滚动异常
            }
        }

        // 处理接收到的消息
        public void HandleMessage(string message)
        {
            if (_stateMachine != null)
            {
                _stateMachine.HandleMessage(message);
            }
        }

        // 更新设备周期
        public void UpdateDeviceCycle(int cycle)
        {
            this.Invoke(new Action(() =>
            {
                input3.Text = cycle.ToString();
            }));
        }

        // 清空日志
        public void ClearLog()
        {
            this.Invoke(new Action(() =>
            {
                dataTable.Rows.Clear();
                AddLogMessage("日志已清空");
            }));
        }
    }
}

