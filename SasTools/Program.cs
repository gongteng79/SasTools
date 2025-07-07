using System;
using System.Windows.Forms;
using WpFramework.LogFactory;

namespace SasTools
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]//单线程单元模型(Single-Threaded Apartment)
        static void Main()
        {
            Application.EnableVisualStyles();//配置应用程序的视觉样式
            Application.SetCompatibleTextRenderingDefault(false);//配置应用程序的文本渲染
            Log.Configure();//初始化日志系统
            Application.Run(new OverView());//启动主窗体
        }
    }
}
