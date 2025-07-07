using System;
using System.Collections.Generic;
using System.Drawing;
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
            _deviceManagementView = new SasTools.UI.DeviceManagementView(_eventBus);

            // 先获取DeviceManager实例
            _deviceManager = _deviceManagementView.GetDeviceManager();

            // 然后创建需要DeviceManager的视图
            _fatigueTestView = new FatigueTestView(_eventBus, _deviceManager);

            InitialCompoent();
        }

        // 添加获取DeviceManager的方法
        public DeviceManager GetDeviceManager()
        {
            return _deviceManager;
        }

        private void InitialCompoent()
        {
            InitializeComponent();
            this.menu1.SelectIndex(0);// 默认选中第一个菜单项

            // 禁用连接按钮并更新显示
            btnAddDevice.Text = "请在设备管理中连接设备";
            btnAddDevice.Enabled = false;
            btnAddDevice.Type = AntdUI.TTypeMini.Default;
            windowBar.SubText = "请使用设备管理功能";
        }

        //切换子视图
        private void ChangeSubView(int index)
        {
            if (index >= 0)
            {
                Control ctrl;
                //视图获取/创建
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
                    prev.Hide();//只隐藏不销毁
                }

                //视图切换
                ctrl.Dock = DockStyle.Fill;//Fill模式确保视图已填满
                this.pnlView.Controls.Clear();//清理容器
                this.pnlView.Controls.Add(ctrl);//添加新视图

                ctrl.Show();//显示触发显示
            }

            prevIndex = index;
        }

        //工厂方法模式 创建视图
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
    }
}

