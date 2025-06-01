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
using SasTools.Events;
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
        private readonly IEventBus _eventBus;
        private readonly ILog _logger = LogManager.GetLogger("OverView");
        private int prevIndex = -1;
        private Dictionary<int, Control> subViews = new Dictionary<int, Control>();
        private ICommunicationService _communicationService;
        private bool _isConnected = false;
        private string _defaultHost = "192.168.2.12";
        private int _defaultPort = 6062;

        private readonly MainView _mainVeiw;
        private readonly ManualView _manualView;
        private readonly ReciepeView _reciepeView;
        private readonly TestTcpView _testTcpView;
        private readonly FatigueTestView _fatigueTestView;
        private IDevice _sasTest;

        public OverView()
        {
            _eventBus = new EventBus(false);
            _mainVeiw = new MainView();
            _manualView = new ManualView();
            _reciepeView = new ReciepeView();
            _testTcpView = new TestTcpView(_eventBus);
            _fatigueTestView = new FatigueTestView(_eventBus);
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
                    ctrl = _mainVeiw;
                    break;

                case 1:
                    ctrl = _manualView;
                    break;

                case 2:
                    ctrl = _reciepeView;
                    break;

                case 3:
                    ctrl = _testTcpView;
                    break;

                case 4:
                    ctrl = _mainVeiw;
                    break;

                case 5:
                    ctrl = _fatigueTestView;
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

        private async void btnAddDevice_Click(object sender, EventArgs e)
        {
            try
            {
                AntdUI.Button btn = (AntdUI.Button)sender;
                btn.Loading = true;

                if (!_isConnected)
                {
                    CreatDevice();
                    bool result = _sasTest.ConnectServer();
                    string message = result ? "设备连接成功" : "设备连接失败";
                    _logger.Info(message);

                    if (result)
                    {
                        this._eventBus.Publish(new DeviceCreateEvent(_sasTest));
                        AntdUI.Message.success(this, message);
                    }
                    else
                    {
                        DeleteDevice();
                        AntdUI.Message.error(this, message);
                    }
                }
                else
                {
                    bool result = await _communicationService.DisconnectAsync();
                    DeleteDevice();
                    string message = result ? "设备断开连接成功" : "设备断开连接失败";
                    _logger.Info(message);

                    AntdUI.Message.info(this, message);
                }

                btn.Loading = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"设备连接操作出错: {ex.Message}", ex);
                AntdUI.Message.error(this, "设备连接操作出错");

                ((AntdUI.Button)sender).Loading = false;
            }
        }

        private void CreatDevice()
        {
            _communicationService = new CommunicationService(_defaultHost, _defaultPort);
            _communicationService.ConnectionStatusChanged += CommunicationService_ConnectionStatusChanged;
            _sasTest = new SasDevice(_communicationService, _eventBus);
        }

        private void DeleteDevice()
        {
            _sasTest = null;
            _communicationService = null;
        }
    }
}
