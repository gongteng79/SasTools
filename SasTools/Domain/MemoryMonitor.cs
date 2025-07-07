using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace SasTools.Domain
{
    public class MemoryMonitor : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(MemoryMonitor));
        private readonly Timer _monitorTimer;
        private readonly long _warningThresholdMB;//内存警告阈值
        private readonly long _criticalThresholdMB;//临界阈值
        private bool _disposed = false;

        public event EventHandler<MemoryWarningEventArgs> MemoryWarning;

        public MemoryMonitor(long warningThresholdMB = 500, long criticalThresholdMB = 800)
        {
            _warningThresholdMB = warningThresholdMB;
            _criticalThresholdMB = criticalThresholdMB;

            // 每30秒检查一次内存使用
            _monitorTimer = new Timer(CheckMemoryUsage, null,TimeSpan.FromSeconds(30),TimeSpan.FromSeconds(30));
        }

        //核心监控逻辑
        private void CheckMemoryUsage(object state)
        {
            if (_disposed) return;

            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    long memoryMB = process.WorkingSet64 / (1024 * 1024);

                    if (memoryMB > _criticalThresholdMB)
                    {
                        _logger.Warn($"内存使用达到临界值: {memoryMB}MB (临界值: {_criticalThresholdMB}MB)");

                        OnMemoryWarning(new MemoryWarningEventArgs(memoryMB, MemoryWarningLevel.Critical));
                    }
                    else if (memoryMB > _warningThresholdMB)
                    {
                        _logger.Info($"内存使用达到警告值： {memoryMB}MB (警告值: {_warningThresholdMB}MB)");
                        OnMemoryWarning(new MemoryWarningEventArgs(memoryMB, MemoryWarningLevel.Warning));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"内存监控检查失败 : {ex.Message}",ex);
            }
        }

        protected virtual void OnMemoryWarning(MemoryWarningEventArgs e)
        {
            MemoryWarning?.Invoke(this,e);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _monitorTimer.Dispose();
                _disposed = true;
            }
        }

        public class MemoryWarningEventArgs : EventArgs
        {
            public long MemoryUsageMB { get; }
            public MemoryWarningLevel Level { get; }

            public MemoryWarningEventArgs(long memoryUsageMB, MemoryWarningLevel level)
            {
                MemoryUsageMB = memoryUsageMB;
                Level = level;
            }
        }
        public enum MemoryWarningLevel
        {
            Warning,
            Critical
        }
    }


}
