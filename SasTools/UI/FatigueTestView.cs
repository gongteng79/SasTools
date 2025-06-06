using log4net;
using SasTools.Domain;
using SasTools.Interface;
using System;
using System.Data;
using System.Windows.Forms;
using WpFramework.EventBus;
using SasTools.Events;

namespace SasTools.UI
{
    public partial class FatigueTestView : UserControl, IEventHandler<RefreshMachineState>, IEventHandler<DeviceCreateEvent>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(FatigueTestView));
        private TestStateMachine _stateMachine;
        private IDevice _sasTest;
        private FatigueParams _parameter;
        private IEventBus _eventBus;

        private DataTable dataTable;

        public FatigueTestView(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<RefreshMachineState>(this);
            _eventBus.Subscribe<DeviceCreateEvent>(this);
            _parameter = new FatigueParams();
            dataTable = new DataTable();
            InitializeComponent();
            InitialInput();

            dataTable.Columns.Add("Time", typeof(string));
            dataTable.Columns.Add("State", typeof(string));
            dataTable.Columns.Add("Message", typeof(string));
            this.tableAlarmInfo.DataSource = dataTable;
        }

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
            }
            catch (Exception ex)
            {
                _logger.Error($"停止测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"停止测试发生错误: {ex.Message}");
            }
        }

        private void AddLogMessage(string state, string message)
        {
            DataRow row = dataTable.NewRow();
            row["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["State"] = state;
            row["Message"] = message;
            dataTable.Rows.Add(row);
            if (dataTable.Rows.Count > 100)
            {
                dataTable.Rows.RemoveAt(0);
            }
            this.tableAlarmInfo.SelectedIndex = this.dataTable.Rows.Count - 1;
            this.tableAlarmInfo.ScrollLine(this.tableAlarmInfo.SelectedIndex);
            this.tableAlarmInfo.Refresh();
        }

        void IEventHandler<RefreshMachineState>.Handle(RefreshMachineState evt)
        {
            this.BeginInvoke(new Action(() => AddLogMessage(evt.Status, evt.Message)));
        }

        void IEventHandler<DeviceCreateEvent>.Handle(DeviceCreateEvent evt)
        {
            _sasTest = evt.SasDevice;
            _stateMachine = new TestStateMachine(_sasTest, _parameter, _eventBus);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this._parameter.ForwardDelay = (int)this.txtForwardDelay.Value;
            this._parameter.ReverseDelay = (int)this.txtReverseDelay.Value;
            this._parameter.Timeout = (int)this.txtRotationTimes.Value;
        }

        private void InitialInput()
        {
            this.txtForwardDelay.Value = this._parameter.ForwardDelay;
            this.txtReverseDelay.Value = this._parameter.ReverseDelay;
            this.txtRotationTimes.Value = this._parameter.Timeout;
        }
    }
}

