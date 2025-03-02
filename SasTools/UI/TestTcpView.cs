using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Domain;
using SasTools.Events;
using SasTools.Interface;
using SasTools.Models.SasModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WpFramework.EventBus;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SasTools.UI
{
    public partial class TestTcpView : UserControl, IEventHandler<SendDataEvent>
    {
        private readonly ISasTest _sas;
        private readonly IEventBus _eventBus;
        private RequestData RequestData;

        public TestTcpView(ISasTest sas, IEventBus eventBus)
        {
            _sas = sas;
            _eventBus = eventBus;
            this._eventBus.Subscribe(this);
            InitializeComponent();

            // 将 FunctionType 枚举值与中文描述添加到 ComboBox
            foreach (var kvp in FunctionTypeDescriptions.Descriptions)
            {
                this.functionComboBox.Items.Add(new ComboBoxItem(kvp.Key, kvp.Value));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AntdUI.Button btn = (AntdUI.Button)sender;
            btn.Loading = true;
            bool change = false;
            AntdUI.ITask.Run(() =>
            {
                var data = this._sas.ReadData(RequestData);
                if (btn.IsDisposed) return;
                btn.Invoke(new Action(() =>
                {
                    if (btn.IsDisposed) return;
                    btn.Loading = false;
                }));
            });
        }

        void IEventHandler<SendDataEvent>.Handle(SendDataEvent evt)
        {
            // 将 evt 的消息格式化并显示在 TextBox 上
            if (evt != null)
            {
                Action action = () =>
                {
                    textBox1.Text += $"{evt.TimeStamp}: 发送: {evt.SendData}{Environment.NewLine}";
                    textBox1.Text += $"{evt.TimeStamp}: 接收: {evt.RecieveData}{Environment.NewLine}";
                };
                this.Invoke(action);

            }
        }

        private void functionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = this.functionComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                FunctionType selectedFunction = selectedItem.FunctionType;
                RequestData = DataFactory.CreateData(selectedFunction);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var result = this._sas.ConnectServer();

            if (result)
            {
                MessageBox.Show("连接成功");
                textBox1.Text += $"{DateTime.Now}: 连接成功{Environment.NewLine}";
            }
            else
            {
                MessageBox.Show("连接失败");
                textBox1.Text += $"{DateTime.Now}: 连接失败{Environment.NewLine}";
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            var result = this._sas.DisconnectServer();

            if (result)
            {
                MessageBox.Show("断开连接成功");
                textBox1.Text += $"{DateTime.Now}: 断开连接成功{Environment.NewLine}";
            }
            else
            {
                MessageBox.Show("断开连接失败");
                textBox1.Text += $"{DateTime.Now}: 断开连接失败{Environment.NewLine}";
            }
        }
    }

    public static class FunctionTypeDescriptions
    {
         public static readonly Dictionary<FunctionType, string> Descriptions = new Dictionary<FunctionType, string>
        {
            { FunctionType.ControlIdleRun, "控制螺丝锁付空转运行" },
            { FunctionType.RemoveScrewAction, "拆螺丝" },
            { FunctionType.LockScrewAction, "锁螺丝" },
            { FunctionType.TorqueTest, "扭力测试" },
            { FunctionType.Stop, "停止" },
            { FunctionType.StatusQuery, "状态查询" },
            { FunctionType.ProductInfoQuery, "产品信息查询" },
            { FunctionType.TorqueCalibration, "扭力标定" },
            { FunctionType.MotorSelfTest, "电机自检" },
            { FunctionType.LockMode, "锁付模式" },
            { FunctionType.PowerControl, "电批通电控制" },
            { FunctionType.TightenInfoControl, "锁付信息控制" },
            { FunctionType.WriteBarcode, "写入条码" }
        };
    }

    public class ComboBoxItem
    {
        public FunctionType FunctionType { get; }
        public string Description { get; }

        public ComboBoxItem(FunctionType functionType, string description)
        {
            this.FunctionType = functionType;
            this.Description = description;
        }

        public override string ToString()
        {
            return this.Description;
        }
    }
}
