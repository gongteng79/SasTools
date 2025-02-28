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
using WpFramework.LogFactory;

namespace SasTools
{
    public partial class OverView: Window
    {
        public OverView()
        {
            InitialCompoent();
        }

        private void InitialCompoent()
        {
            InitializeComponent();
            this.menu1.SelectIndex(0);
        }
    }
}
