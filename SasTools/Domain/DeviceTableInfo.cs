using System;
using System.Data;
using System.Windows.Forms;
using AntdUI;
using SasTools.Events;
namespace SasTools.Domain
{
    public class DeviceTableInfo:IDisposable
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public AntdUI.Table Table { get; set; }
        public DataTable DataSource { get; set; }
        public AntdUI.Panel Container { get; set; }//容器面板(包装表格)

        //计数器数据
        public int TotalCycles { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }

        //管理信息
        public DateTime LastAccessTime { get; set;}//最后访问时间
        public bool IsVisible { get; set; }//当前是否显示在界面中

        // 设备运行状态
        public bool IsTestRunning { get; set; } = false;

        // 设备状态信息
        public MachineStatusType CurrentStatus { get; set; } = MachineStatusType.Idle;
        public string StatusText { get; set; } = "空闲";
        public string StatusMessage { get; set; } = "";

        // UI状态信息
        public AntdUI.TState BadgeState { get; set; } = AntdUI.TState.Default;

        public bool _disposed = false;//防止重复释放

        public void Dispose()
        {
            if (!_disposed)
            {
                //清理UI控件
                Container?.Controls.Clear();
                Container?.Dispose();

                //清理数据
                DataSource?.Clear();
                DataSource?.Dispose();

                //清理表格
                Table?.Dispose();

                _disposed = true;
            }
        }

        public void UpdateCounters(int totalCycles, int successCount, int failureCount)
        {
            TotalCycles = totalCycles;
            SuccessCount = successCount;
            FailureCount = failureCount;
            LastAccessTime = DateTime.Now;
        }
    }
}
