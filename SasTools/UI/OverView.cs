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
        //依赖项
        private readonly IEventBus _eventBus;//事件总线
        private readonly ILog _logger = LogManager.GetLogger("OverView");
        private int prevIndex = -1;//记录之前选中的菜单索引
        //设备管理
        private DeviceManager _deviceManager;
        private string _selectedDeviceId;//当前选中的设备ID
        //视图组件
        private Dictionary<int, Control> subViews = new Dictionary<int, Control>();//存储所有子视图的字典，实现视图缓存
        private readonly MainView _mainVeiw;
        private readonly ManualView _manualView;
        private readonly ReciepeView _reciepeView;
        private readonly TestTcpView _testTcpView;
        private readonly FatigueTestView _fatigueTestView;
        private SasTools.UI.DeviceManagementView _deviceManagementView;
        private IDevice _sasTest;//测试设备接口

        //初始化流程
        public OverView()
        {
            //初始化事件总线
            _eventBus = new EventBus(false);
            //初始化各视图
            _mainVeiw = new MainView();
            _manualView = new ManualView();
            _reciepeView = new ReciepeView();
            _testTcpView = new TestTcpView(_eventBus);
            _fatigueTestView = new FatigueTestView(_eventBus);
            _deviceManagementView = new SasTools.UI.DeviceManagementView(_eventBus);

            // 建立设备管理与主界面的连接(事件订阅)
            _deviceManagementView.DeviceSelectedForConnection += OnDeviceSelectedForConnection;
            _deviceManager = _deviceManagementView.GetDeviceManager(); // 共享DeviceManager实例

            // 订阅设备状态变化事件
            _deviceManager.DeviceStatusChanged += OnDeviceStatusChanged;

            InitialCompoent();
        }

        // 当设备管理视图中选择设备进行连接时触发
        private void OnDeviceSelectedForConnection(object sender, string deviceId)
        {
            SetSelectedDevice(deviceId);// 设置当前选中的设备
        }

        // 当设备状态发生变化时触发
        private void OnDeviceStatusChanged(object sender, SasTools.Services.DeviceStatusChangedEventArgs e)
        {
            // 如果变化的是当前选中的设备，更新主界面按钮
            if (e.DeviceId == _selectedDeviceId)
            {
                // 确保在UI线程上执行
                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => UpdateConnectionButtonState()));
                }
                else
                {
                    UpdateConnectionButtonState();
                }
            }
        }

        private void InitialCompoent()
        {
            InitializeComponent();
            this.menu1.SelectIndex(0);// 默认选中第一个菜单项

            // 初始化连接按钮状态
            UpdateConnectionButtonState();
        }

        //切换子视图
        private void ChangeSubView(int index)
        {
            if (index >= 0)
            {
                Control ctrl;
                bool succ = this.subViews.TryGetValue(index, out ctrl);
                if (!succ)// 如果视图不存在于缓存中
                {
                    ctrl = this.CreateSubView(index);// 创建新视图
                    this.subViews.Add(index, ctrl);// 加入缓存
                }

                //隐藏之前的视图
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

                case 6:
                    ctrl = _deviceManagementView;
                    break;

                default:
                    break;
            }

            return ctrl;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            LoginView loginView = new LoginView();// 创建登录视图
            var align = AntdUI.TAlignMini.Right;// 右侧对齐
            loginView.Size = new Size(500, 100);// 设置大小
            AntdUI.Drawer.open(this, loginView, align);// 以抽屉形式打开
        }

        //菜单选择变化
        private void menu1_SelectChanged(object sender, MenuSelectEventArgs e)
        {
            var item = menu1.GetSelectItem();// 获取选中项
            int index = menu1.GetSelectIndex(item);// 获取索引
            if (index >= 0)
            {
                this.ChangeSubView(index);// 切换视图
            }
        }


        // 添加/连接设备按钮点击
        private async void btnAddDevice_Click(object sender, EventArgs e)
        {
            try
            {
                AntdUI.Button btn = (AntdUI.Button)sender;
                btn.Loading = true;//显示加载状态

                if (string.IsNullOrEmpty(_selectedDeviceId))
                {
                    // 如果没有选中设备，提示用户先添加设备
                    AntdUI.Message.info(this, "请先在设备管理中添加并选择设备");
                    // 自动跳转到设备管理页面
                    this.menu1.SelectIndex(6); // 设备管理页面索引
                    btn.Loading = false;
                    return;
                }

                //获取当前设备选中的信息
                var deviceInfo = _deviceManager.GetDevice(_selectedDeviceId);
 
                if (deviceInfo == null)
                {
                    AntdUI.Message.error(this, "设备不存在");
                    btn.Loading = false;
                    return;
                }

                if (!deviceInfo.IsConnected)
                {
                    //连接设备
                    bool result = await _deviceManager.ConnectDevice(_selectedDeviceId);
                    string message = result ? "设备连接成功" : "设备连接失败";
                    _logger.Info(message);

                    if (result)
                    {
                        var device = _deviceManager.GetDeviceInstance(_selectedDeviceId);
                        if (device != null)
                        {
                            this._eventBus.Publish(new DeviceCreateEvent(device));
                        }
                        AntdUI.Message.success(this, message);
                    }
                    else
                    {
                        AntdUI.Message.error(this, message);
                    }
                }
                else
                {
                    //断开设备
                    bool result = await _deviceManager.DisconnectDevice(_selectedDeviceId);
                    string message = result ? "设备断开成功" : "设备断开失败";
                    _logger.Info(message);
                    AntdUI.Message.info(this, message);
                }

                UpdateConnectionButtonState();
                btn.Loading = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"设备连接操作出错: {ex.Message}", ex);
                AntdUI.Message.error(this, "设备连接操作出错");
                ((AntdUI.Button)sender).Loading = false;
            }
        }

        // 设置当前选中的设备
        public void SetSelectedDevice(string deviceId)
        {
            _selectedDeviceId = deviceId;
            UpdateConnectionButtonState();//更新按钮状态
        }

        // 更新连接按钮状态
        private void UpdateConnectionButtonState()
        {
            //如果未连接任何设备
            if (string.IsNullOrEmpty(_selectedDeviceId))
            {
                btnAddDevice.Text = "选择设备"; //按钮显示：选择设备 
                btnAddDevice.Type = AntdUI.TTypeMini.Default;//默认灰色样式
                btnAddDevice.Enabled = false;//禁用按钮
                windowBar.SubText = "未选择设备";//状态栏显示
                return;
            }


            //设备存在的情况
            var deviceInfo = _deviceManager?.GetDevice(_selectedDeviceId);
            if (deviceInfo != null)
            {
                //动态文本
                btnAddDevice.Text = deviceInfo.IsConnected ? "断开设备" : "连接选中设备";
                btnAddDevice.Type = deviceInfo.IsConnected ? AntdUI.TTypeMini.Error : AntdUI.TTypeMini.Primary;
                btnAddDevice.Enabled = true;
                windowBar.SubText = $"当前设备: {deviceInfo.Name} ({deviceInfo.Host}:{deviceInfo.Port})";
            }
            //设备不存在的
            else
            {
                btnAddDevice.Text = "设备不存在";
                btnAddDevice.Type = AntdUI.TTypeMini.Default;//默认恢复演示
                btnAddDevice.Enabled = false;
                windowBar.SubText = "设备不存在";    
            }
        }
    }
}

