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
using WpFramework.EventBus;
using SasTools.Events;

namespace SasTools.UI
{
    public partial class FatigueTestView : UserControl, IEventHandler<SendDataEvent>, IEventHandler<DeviceCreateEvent>
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(FatigueTestView));
        private TestStateMachine _stateMachine;
        private IDevice _sasTest;
        private IParameterService _parameterService;
        private IEventBus _eventBus;

        private DataTable dataTable;

        public FatigueTestView(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<SendDataEvent>(this);
            _eventBus.Subscribe<DeviceCreateEvent>(this);
            InitializeComponent();
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

        private void AddLogMessage(string message)
        {
            DataRow row = dataTable.NewRow();
            row["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["Info"] = message;
            dataTable.Rows.Add(row);
            if (dataTable.Rows.Count > 100)
            {
                dataTable.Rows.RemoveAt(0);
            }
        }

        public void ClearLog()
        {
            this.Invoke(new Action(() =>
            {
                dataTable.Rows.Clear();
                AddLogMessage("日志已清空");
            }));
        }

        void IEventHandler<SendDataEvent>.Handle(SendDataEvent evt)
        {
            this.BeginInvoke(new Action(() => AddLogMessage(evt.RecieveData)));
        }

        void IEventHandler<DeviceCreateEvent>.Handle(DeviceCreateEvent evt)
        {
            _sasTest = evt.SasDevice;
            _stateMachine = new TestStateMachine(_sasTest, _parameterService, _eventBus);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Stop();
        }
    }
}

