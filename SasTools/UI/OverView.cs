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
using log4net;
using SasTools.Domain;
using SasTools.Interface;
using SasTools.Models.Communication;
using SasTools.Models.SasModule;
using SasTools.Services;
using SasTools.UI;
using WpFramework.EventBus;
using WpFramework.LogFactory;

namespace SasTools
{
    public partial class OverView : Window
    {
        private IEventBus eventBus;
        private readonly ILog _logger = LogManager.GetLogger("OverView");
        private int prevIndex = -1;
        private Dictionary<int, Control> subViews = new Dictionary<int, Control>();
        private ICommunicationService _communicationService;
        private bool _isConnected = false;
        private string _defaultHost = "192.168.2.12";
        private int _defaultPort = 6062;

        public OverView()
        {
            eventBus = new EventBus(false);
            // 初始化通信服务，使用依赖注入模式
            _communicationService = new CommunicationService(_defaultHost, _defaultPort);
            _communicationService.ConnectionStatusChanged += CommunicationService_ConnectionStatusChanged;
            _communicationService.MessageReceived += CommunicationService_MessageReceived;
            InitialCompoent();
        }

        private void CommunicationService_ConnectionStatusChanged(object sender, bool isConnected)
        {
            _isConnected = isConnected;

            // 更新 UI 以反映连接状态
            this.Invoke(new Action(() =>
            {
                btnAddDevice.Text = isConnected ? "断开设备" : "连接设备";
                btnAddDevice.Type = isConnected ? AntdUI.TTypeMini.Error : AntdUI.TTypeMini.Primary;
                _logger.Info(isConnected ? "设备已连接" : "设备已断开连接");

                // 可选：更新状态栏或其他 UI 元素
                windowBar.SubText = isConnected ? $"已连接 {_defaultHost}:{_defaultPort}" : "未连接";
            }));
        }

        private void CommunicationService_MessageReceived(object sender, string message)
        {
            // 处理接收到的消息，例如记录日志
            _logger.Debug($"收到消息: {message}");
        }

        private void InitialCompoent()
        {
            InitializeComponent();
            this.menu1.SelectIndex(0);

            // 初始化连接按钮状态
            btnAddDevice.Text = "连接设备";
            btnAddDevice.Type = AntdUI.TTypeMini.Primary;
        }

        private void ChangeSubView(int index)
        {
            if (index >= 0)
            {
                Control ctrl;
                bool succ = this.subViews.TryGetValue(index, out ctrl);
                if (!succ)
                {
                    ctrl = this.CreateSubView(index);
                    this.subViews.Add(index, ctrl);
                }

                Control prev;
                succ = this.subViews.TryGetValue(prevIndex, out prev);
                if (succ && prev != null && prevIndex != index)
                {
                    prev.Hide();
                }

                ctrl.Dock = DockStyle.Fill;
                this.pnlView.Controls.Clear();
                this.pnlView.Controls.Add(ctrl);

                ctrl.Show();
            }

            prevIndex = index;
        }

        private Control CreateSubView(int index)
        {
            Control ctrl = null;

            switch (index)
            {
                case 0:
                    ctrl = new MainView();
                    break;

                case 1:
                    ctrl = new DataSetView();
                    break;

                case 2:
                    ctrl = new ReciepeView();
                    break;

                case 3:
                    // 使用共享的通信服务实例
                    SasTest sasTest = new SasTest(_communicationService, eventBus);
                    ctrl = new TestTcpView(sasTest, eventBus);
                    break;

                case 4:
                    ctrl = new ManualView();
                    break;

                case 5:
                    ctrl = new AbnormalAlarmView();
                    break;

                default:
                    break;
            }

            return ctrl;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            LoginView loginView = new LoginView();
            var align = AntdUI.TAlignMini.Right;
            loginView.Size = new Size(500, 100);
            AntdUI.Drawer.open(this, loginView, align);
        }

        private void menu1_SelectChanged(object sender, MenuSelectEventArgs e)
        {
            var item = menu1.GetSelectItem();
            int index = menu1.GetSelectIndex(item);
            if (index >= 0)
            {
                this.ChangeSubView(index);
            }
        }

        // 实现设备连接/断开功能
        private async void btnAddDevice_Click(object sender, EventArgs e)
        {
            try
            {
                AntdUI.Button btn = (AntdUI.Button)sender;
                btn.Loading = true; // 设置按钮为加载状态

                if (!_isConnected)
                {
                    // 可以添加连接配置对话框（可选）
                    // CreateDeviceView deviceView = new CreateDeviceView();
                    // if (deviceView.ShowDialog() == DialogResult.OK)
                    // {
                    //     _defaultHost = deviceView.Host;
                    //     _defaultPort = deviceView.Port;
                    // }

                    // 连接设备
                    bool result = await _communicationService.ConnectAsync(_defaultHost, _defaultPort);
                    string message = result ? "设备连接成功" : "设备连接失败";
                    _logger.Info(message);

                    // 显示消息通知
                    if (result)
                    {
                        AntdUI.Message.success(this, message);
                    }
                    else
                    {
                        AntdUI.Message.error(this, message);
                    }
                }
                else
                {
                    // 断开设备连接
                    bool result = await _communicationService.DisconnectAsync();
                    string message = result ? "设备断开连接成功" : "设备断开连接失败";
                    _logger.Info(message);

                    // 显示消息通知
                    AntdUI.Message.info(this, message);
                }

                btn.Loading = false; // 取消按钮加载状态
            }
            catch (Exception ex)
            {
                _logger.Error($"设备连接操作出错: {ex.Message}", ex);
                AntdUI.Message.error(this, "设备连接操作出错");

                // 确保按钮恢复正常状态
                ((AntdUI.Button)sender).Loading = false;
            }
        }
    }
}
