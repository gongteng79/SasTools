using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using log4net;
using SasTools.Interface;
using SasTools.Services;
using SasTools.UI;
using WpFramework.EventBus;

namespace SasTools
{
    public partial class OverView : Window
    {
        //依赖项
        private readonly IEventBus _eventBus;//事件总线
        private readonly ILog _logger = LogManager.GetLogger(typeof(OverView));
        private int prevIndex = -1;//记录之前选中的菜单选项

        //设备管理
        private DeviceManager _deviceManager;

        //视图组件
        private Dictionary<int, Control> subView = new Dictionary<int, Control>();//存储
        private readonly ManualView _manualView;
        private readonly MainView _mainView;
        private readonly TestTcpView _testTcpview;
        private readonly ReciepeView _reciepeView;
        private readonly FatigueTestView _fatigueTestView;
        private SasTools.UI.DeviceManagementView _deviceManagermentView;
        private IDevice sasTest;//测试设备接口

        //初始化构造函数
        public OverView()
        {
            _eventBus = new EventBus(false);//初始化事件总线
            _manualView = new ManualView();
            _mainView = new MainView();
            _testTcpview = new TestTcpView(_eventBus);
            _reciepeView = new ReciepeView();
            _deviceManagermentView = new DeviceManagementView(_eventBus);
            //先获取DeviceManager实例
            _deviceManager = _deviceManagermentView.GetDeviceManager();

            _fatigueTestView = new FatigueTestView(_eventBus, _deviceManager);

            InitialCompoent();//初始化组件
        }

        public void InitialCompoent()
        {
            InitializeComponent();//初始化设计器生成的组件
            this.menu1.SelectIndex(0);

            //禁用连接按钮
            btnAddDevice.Text = "请在设备管理中";
            btnAddDevice.Enabled = false;
            btnAddDevice.Type = AntdUI.TTypeMini.Default;
        }

        //切换视图
        private void ChangeSubView(int Index)
        {
            if (Index >= 0)
            {
                Control ctrl;
                bool succ = this.subView.TryGetValue(Index, out ctrl);
                //如果试图不存在于缓存中
                if (!succ)
                {
                    //创建视图
                    ctrl = this.CreateSubView(Index);
                    //加入缓存
                    this.subView.Add(Index,ctrl);
                }

                //隐藏之前的视图
                Control prev;
                succ = this.subView.TryGetValue(prevIndex,out prev);
                if (succ && prev != null && prevIndex != Index)
                {
                    prev.Hide();
                }

                //视图切换
                ctrl.Dock = DockStyle.Fill;
                this.pnlView.Controls.Clear();
                this.pnlView.Controls.Add(ctrl);
                ctrl.Show();
            }
            prevIndex = Index;//更新前一个索引
        }

        private Control CreateSubView(int index)
        {
            Control ctrl = null;
            switch (index)
            {
                case 0:
                    ctrl = _mainView;
                    break;
                case 1:
                    ctrl = _manualView;
                    break;
                case 2:
                    ctrl = _reciepeView;
                    break;
                case 3:
                    ctrl = _testTcpview;
                    break;
                case 4:
                    ctrl = _mainView;
                    break;
                case 5:
                    ctrl = _fatigueTestView;
                    break;
                case 6:
                    ctrl = _deviceManagermentView;
                    break;
                default:
                    break;
            }
            return ctrl;
        }

        //用户登录按钮
        // 登录按钮点击事件
        private void btnLogin_Click(object sender, EventArgs e)
        {
            LoginView loginView = new LoginView(); // 创建登录视图
            var align = AntdUI.TAlignMini.Right; // 右侧对齐
            loginView.Size = new Size(500, 100); // 设置大小
            AntdUI.Drawer.open(this, loginView, align); // 以抽屉形式打开
        }


        //菜单变化选择
        private void menu1_SelectChanged(object sender, MenuSelectEventArgs e)
        {
            var item = menu1.GetSelectItem();//获取当前选中的
            int index = menu1.GetSelectIndex(item);
            if (index >= 0)
            {
                this.ChangeSubView(index);
            }

        }
    }
}

