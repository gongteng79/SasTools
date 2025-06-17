using log4net;
using SasTools.Domain;
using SasTools.Interface;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WpFramework.EventBus;
using SasTools.Events;

namespace SasTools.UI
{
    public partial class FatigueTestView : UserControl, IEventHandler<RefreshMachineState>, IEventHandler<DeviceCreateEvent>, IEventHandler<CounterUpdateEvent>
    {
        #region 私有字段
        private readonly ILog _logger = LogManager.GetLogger(typeof(FatigueTestView));
        private TestStateMachine _stateMachine;
        private IDevice _sasTest;
        private FatigueParams _parameter;
        private IEventBus _eventBus;
        private DataTable _dataTable;
        private bool _isTestRunning = false;
        #endregion

        #region 构造函数
        public FatigueTestView(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<RefreshMachineState>(this);
            _eventBus.Subscribe<DeviceCreateEvent>(this);
            _eventBus.Subscribe<CounterUpdateEvent>(this);

            _parameter = new FatigueParams();
            _dataTable = new DataTable();

            InitializeComponent();
            InitializeDataTable();
            InitialInput();

            // 设置UI控件初始状态
            UpdateUIState(false);
        }
        #endregion

        #region 初始化方法
        private void InitializeDataTable()
        {
            _dataTable.Columns.Add("Time", typeof(string));
            _dataTable.Columns.Add("State", typeof(string));
            _dataTable.Columns.Add("Message", typeof(object));
            _dataTable.Columns.Add("StatusType", typeof(string));

            // 设置表格列配置
        var columns = new AntdUI.ColumnCollection
        {
            new AntdUI.Column("Time", "时间"),
            new AntdUI.Column("State", "状态"),
            new AntdUI.Column("Message", "消息"),
            new AntdUI.Column("StatusType", "状态类型")
        };

            this.tableAlarmInfo.Columns = columns;
            this.tableAlarmInfo.DataSource = _dataTable;
        }

        private void InitialInput()
        {
            // 先使用默认值初始化UI
            this.txtForwardDelay.Value = _parameter.ForwardDelay;
            this.txtReverseDelay.Value = _parameter.ReverseDelay;
            this.txtRotationTimes.Value = _parameter.Timeout;
            this.inputNumber1.Value = _parameter.MaxCycles;
            this.inputNumber2.Value = _parameter.MaxFailures;

            // 在后台线程中加载参数
            Task.Run(() => {
                try
                {
                    var parameters = _parameter.LoadParameterAsync().Result;

                    // 在UI线程中更新控件
                    this.BeginInvoke(new Action(() => {
                        this.txtForwardDelay.Value = parameters.ForwardDelay;
                        this.txtReverseDelay.Value = parameters.ReverseDelay;
                        this.txtRotationTimes.Value = parameters.Timeout;
                        this.inputNumber1.Value = parameters.MaxCycles;
                        this.inputNumber2.Value = parameters.MaxFailures;

                        // 更新内存中的参数
                        _parameter.ForwardDelay = parameters.ForwardDelay;
                        _parameter.ReverseDelay = parameters.ReverseDelay;
                        _parameter.Timeout = parameters.Timeout;
                        _parameter.MaxCycles = parameters.MaxCycles;
                        _parameter.MaxFailures = parameters.MaxFailures;
                    }));
                }
                catch (Exception ex)
                {
                    _logger.Error($"加载参数失败: {ex.Message}", ex);
                }
            });
        }
        #endregion

        #region 测试控制方法
        private void Start()
        {
            try
            {
                if (_stateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, "测试系统未初始化");
                    return;
                }

                _stateMachine.StartTest();
                _isTestRunning = true;
                UpdateUIState(true);
                AddLogMessage("系统启动", "测试已开始运行", MachineStatusType.Forward);
            }
            catch (Exception ex)
            {
                _logger.Error($"启动测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"启动测试发生错误: {ex.Message}");
            }
        }

        private void Stop()
        {
            try
            {
                if (_stateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, "测试系统未初始化");
                    return;
                }

                _stateMachine.StopTest();
                _isTestRunning = false;
                UpdateUIState(false);
                AddLogMessage("系统停止", "测试已停止运行", MachineStatusType.Idle);
            }
            catch (Exception ex)
            {
                _logger.Error($"停止测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"停止测试发生错误: {ex.Message}");
            }
        }

        private void Reset()
        {
            try
            {
                if (_stateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, "测试系统未初始化");
                    return;
                }

                // 使用状态机的完整重置方法
                _stateMachine.Reset();
                _isTestRunning = false;

                // 更新UI状态
                UpdateUIState(false);

                // 清空数据表
                _dataTable.Clear();

                // 重置UI显示
                this.badge1.State = AntdUI.TState.Default;
                this.badge1.Text = "空闲";
                this.divider2.Text = "系统已复位";

                // 重置计数器显示
                this.input1.Text = "0";
                this.input2.Text = "0";
                this.input3.Text = "0";

                // 添加复位日志
                AddLogMessage("系统复位", "系统已成功复位，下次启动将从头开始测试", MachineStatusType.Idle);

                // 显示成功消息
                AntdUI.Message.success(this.ParentForm, "系统已成功复位");

                _logger.Info("用户触发系统复位 - 状态机已完全重置");
            }
            catch (Exception ex)
            {
                _logger.Error($"复位系统失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"复位系统发生错误: {ex.Message}");
            }
        }
        #endregion

        #region UI更新方法
        private void UpdateUIState(bool isRunning)
        {
            // 根据测试运行状态更新UI控件状态
            this.button5.Enabled = !isRunning; // 启动按钮
            this.button6.Enabled = isRunning;  // 停止按钮
            this.button7.Enabled = !isRunning; // 复位按钮

            // 测试运行时禁用参数设置
            this.tabPage2.Enabled = !isRunning;

            // 更新状态指示
            if (!isRunning)
            {
                this.badge1.State = AntdUI.TState.Default;
                this.badge1.Text = "空闲";
            }
        }

        private void AddLogMessage(string state, string message, MachineStatusType statusType)
        {
            DataRow row = _dataTable.NewRow();
            row["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["State"] = state;

            // 根据状态类型设置消息的颜色
            string statusText = GetStatusText(statusType);
            row["StatusType"] = statusText;

            // 使用CellText设置消息颜色
            if (statusType == MachineStatusType.Error)
            {
                // 错误消息使用红色
                row["Message"] = new AntdUI.CellText(message)
                {
                    Fore = Color.Red
                };
            }
            else
            {
                // 其他消息使用默认颜色
                row["Message"] = new AntdUI.CellText(message)
                {
                    Fore = Color.Black
                };
            }

            _dataTable.Rows.Add(row);

            // 限制日志条目数量，防止内存占用过大
            if (_dataTable.Rows.Count > 100000)
            {
                _dataTable.Rows.RemoveAt(0);
            }

            // 滚动到最新记录
            this.tableAlarmInfo.SelectedIndex = _dataTable.Rows.Count - 1;
            this.tableAlarmInfo.ScrollLine(this.tableAlarmInfo.SelectedIndex);
            this.tableAlarmInfo.Refresh();
        }

        // 根据状态类型获取状态文本
        private string GetStatusText(MachineStatusType statusType)
        {
            switch (statusType)
            {
                case MachineStatusType.Forward:
                    return "正转";
                case MachineStatusType.Reverse:
                    return "反转";
                case MachineStatusType.Waiting:
                    return "等待";
                case MachineStatusType.Error:
                    return "错误";
                case MachineStatusType.Idle:
                    return "空闲";
                default:
                    return "正常";
            }
        }
        #endregion

        #region 事件处理方法
        void IEventHandler<RefreshMachineState>.Handle(RefreshMachineState evt)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 更新状态指示灯状态  
                    switch (evt.StatusType)
                    {
                        case MachineStatusType.Forward:
                        case MachineStatusType.Reverse:
                            this.badge1.State = AntdUI.TState.Processing; // 运行中  
                            break;
                        case MachineStatusType.Waiting:
                            this.badge1.State = AntdUI.TState.Warn; // 等待中  
                            break;
                        case MachineStatusType.Error:
                            this.badge1.State = AntdUI.TState.Error; // 错误  
                            break;
                        case MachineStatusType.Idle:
                            this.badge1.State = AntdUI.TState.Default; // 空闲  
                            break;
                        default:
                            this.badge1.State = AntdUI.TState.Success; // 正常  
                            break;
                    }

                    // 更新状态文本  
                    this.badge1.Text = GetStatusText(evt.StatusType);
                    this.divider2.Text = evt.Message;

                    // 添加日志条目  
                    AddLogMessage(evt.Status, evt.Message, evt.StatusType);
                }
                catch (Exception ex)
                {
                    _logger.Error($"更新状态显示失败: {ex.Message}", ex);
                }
            }));
        }

        void IEventHandler<DeviceCreateEvent>.Handle(DeviceCreateEvent evt)
        {
            _sasTest = evt.SasDevice;
            _stateMachine = new TestStateMachine(_sasTest, _parameter, _eventBus);

            // 设备创建后添加日志
            this.BeginInvoke(new Action(() => {
                AddLogMessage("设备初始化", "测试设备已成功初始化", MachineStatusType.Idle);
            }));
        }

        //处理计数器更新事件，更新界面显示
        void IEventHandler<CounterUpdateEvent>.Handle(CounterUpdateEvent evt)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 更新总循环次数显示
                    this.input1.Text = evt.TotalCycles.ToString();

                    // 更新成功次数显示
                    this.input2.Text = evt.SuccessfulCycles.ToString();

                    //更新失败次数显示
                    this.input3.Text = evt.FailedCycles.ToString();

                    // 如果达到最大循环次数且不为0，自动停止测试
                    if (_parameter.MaxCycles > 0 && evt.TotalCycles >= _parameter.MaxCycles && _isTestRunning)
                    {
                        Stop();
                        AddLogMessage("测试完成", $"已达到设定的最大循环次数: {_parameter.MaxCycles}", MachineStatusType.Idle);
                    }

                    // 如果失败次数达到上限且不为0，自动停止测试
                    int failedCycles = evt.TotalCycles - evt.SuccessfulCycles;
                    if (_parameter.MaxFailures > 0 && failedCycles >= _parameter.MaxFailures && _isTestRunning)
                    {
                        Stop();
                        AddLogMessage("测试停止", $"已达到设定的最大失败次数: {_parameter.MaxFailures}", MachineStatusType.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"更新计数器显示失败: {ex.Message}", ex);
                }
            }));
        }
        #endregion

        #region 按钮事件处理
        // 电批正转按钮
        private void Button2_Click(object sender, EventArgs e)
        {
            if (_sasTest == null)
            {
                AntdUI.Message.error(this.ParentForm, "测试设备未初始化");
                return;
            }

            _sasTest.ExecuteCommand(SasCommandType.Forward);
            AddLogMessage("手动操作", "执行电批正转", MachineStatusType.Forward);
        }

        // 电批反转按钮
        private void Button3_Click(object sender, EventArgs e)
        {
            if (_sasTest == null)
            {
                AntdUI.Message.error(this.ParentForm, "测试设备未初始化");
                return;
            }

            _sasTest.ExecuteCommand(SasCommandType.Reverse);
            AddLogMessage("手动操作", "执行电批反转", MachineStatusType.Reverse);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            this.Reset();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // 验证参数
                var parameters = new TestParameters
                {
                    ForwardDelay = (int)this.txtForwardDelay.Value,
                    ReverseDelay = (int)this.txtReverseDelay.Value,
                    Timeout = (int)this.txtRotationTimes.Value,
                    MaxCycles = (int)this.inputNumber1.Value,
                    MaxFailures = (int)this.inputNumber2.Value
                };

                if (!_parameter.ValidateParameters(parameters, out string errorMessage))
                {
                    AntdUI.Message.error(this.ParentForm, errorMessage);
                    return;
                }

                // 更新内存中的参数
                _parameter.ForwardDelay = parameters.ForwardDelay;
                _parameter.ReverseDelay = parameters.ReverseDelay;
                _parameter.Timeout = parameters.Timeout;
                _parameter.MaxCycles = parameters.MaxCycles;
                _parameter.MaxFailures = parameters.MaxFailures;

                // 在后台线程中保存参数
                Task.Run(() => {
                    try
                    {
                        _parameter.SaveParameterAsync(parameters).Wait();
                        this.BeginInvoke(new Action(() => {
                            AddLogMessage("参数设置", "参数已成功保存", MachineStatusType.Idle);
                        }));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"保存参数失败: {ex.Message}", ex);
                        this.BeginInvoke(new Action(() => {
                            AntdUI.Message.error(this.ParentForm, $"保存参数失败: {ex.Message}");
                        }));
                    }
                });

                AntdUI.Message.success(this.ParentForm, "参数保存成功");
            }
            catch (Exception ex)
            {
                _logger.Error($"保存参数失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"保存参数失败: {ex.Message}");
            }
        }
        #endregion

    }
}


