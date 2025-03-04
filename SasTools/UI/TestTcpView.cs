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
using static System.Net.Mime.MediaTypeNames;
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
            btn.Loading = true; // 设置按钮为加载状态

            // 异步执行指令发送和接收
            AntdUI.ITask.Run(() =>
            {
                try
                {
                    // 获取选择的指令（需要在 UI 线程中执行）
                    string selectedFunction = string.Empty;
                    functionComboBox.Invoke(new Action(() =>
                    {
                        selectedFunction = functionComboBox.SelectedItem?.ToString();
                    }));

                    if (string.IsNullOrEmpty(selectedFunction))
                    {
                        WriteInfoToBox("请输入有效值！");
                        if (btn.IsDisposed) return; // 检查按钮是否已被释放
                        btn.Loading = false; // 取消按钮加载状态
                        return;
                    }

                    // 调用 _sas.ReadData 发送指令并获取响应
                    var response = this._sas.ReadData(RequestData);

                    // 在 UI 线程中更新按钮状态和显示响应
                    btn.Invoke(new Action(() =>
                    {
                        if (btn.IsDisposed) return; // 检查按钮是否已被释放
                        btn.Loading = false; // 取消按钮加载状态
                    }));
                }
                catch (Exception ex)
                {
                    // 异常处理
                    btn.Invoke(new Action(() =>
                    {
                        WriteInfoToBox(ex.Message);
                        if (btn.IsDisposed) return;
                        btn.Loading = false; 
                    }));
                }
            });
        }

        void IEventHandler<SendDataEvent>.Handle(SendDataEvent evt)
        {
            // 将 evt 的消息格式化并显示在 TextBox 上
            if (evt != null)
            {
                Action action = () =>
                {
                    // 自动滚动到底部
                    textBox1.ScrollToCaret();

                    //显示文本
                    WriteInfoToBox($"发送: {evt.SendData}");
                    WriteInfoToBox($"接收: {evt.RecieveData}");
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
            // 尝试连接设备
            var result = this._sas.ConnectServer();

            // 根据连接结果显示消息
            if (result)
            {
                WriteInfoToBox("连接成功");
            }
            else
            {
                WriteInfoToBox("连接失败");
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            // 尝试断开连接
            var result = this._sas.DisconnectServer();

            // 根据断开连接结果显示消息
            if (result)
            {
                WriteInfoToBox("断开连接成功");
            }
            else
            {
                WriteInfoToBox("断开连接失败");
            }
        }

        /// <summary>
        /// 将消息写入到TextBox上
        /// </summary>
        /// <param name="message"></param>
        private void WriteInfoToBox(string message)
        {
            this.textBox1.ScrollToCaret();
            this.textBox1.AppendText($"{DateTime.Now:G}: {message}{Environment.NewLine}");
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
}
