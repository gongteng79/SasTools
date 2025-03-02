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
using SasTools.UI;
using WpFramework.EventBus;
using WpFramework.LogFactory;

namespace SasTools
{
    public partial class OverView: Window
    {
        private IEventBus eventBus;
        private readonly ILog _logger = LogManager.GetLogger("OverView");
        private int prevIndex = -1;
        private Dictionary<int, Control> subViews = new Dictionary<int, Control>();

        public OverView()
        {
            eventBus = new EventBus(false);
            InitialCompoent();
        }

        private void InitialCompoent()
        {
            InitializeComponent();
            this.menu1.SelectIndex(0);
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
                    var tcpCommunication = CommunicationFactory.CreateTcpCommunication("192.168.1.12", 6062);
                    SasTest sasTest = new SasTest(tcpCommunication, eventBus);
                    ctrl = new TestTcpView(sasTest, eventBus);
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

        private void btnAddDevice_Click(object sender, EventArgs e)
        {
            //CreateDeviceView createView = new CreateDeviceView();
            //var align = AntdUI.TAlignMini.Right;
            //createView.Size = new Size(500, 100);
            //AntdUI.Drawer.open(this, createView, align);

        }
    }
}
