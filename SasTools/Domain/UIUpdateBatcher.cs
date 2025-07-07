using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using SasTools.Domain;

namespace SasTools.Domain
{
    public class UIUpdateBatcher : IDisposable
    {
        private readonly Control _control;
        private readonly System.Threading.Timer _batchTimer;
        private readonly ConcurrentQueue<UIUpdateAction> _updateQueue;
        private readonly object _lockObject = new object();
        private bool _disposed = false;

        public UIUpdateBatcher(Control control, int batchIntervalMs = 50)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _updateQueue = new ConcurrentQueue<UIUpdateAction>();
            _batchTimer = new System.Threading.Timer(ProcessBatchUpdates, null, batchIntervalMs, batchIntervalMs);
        }

        public void QueueUpdate(UIUpdateAction updateAction)
        {
            if (!_disposed && updateAction != null)
            {
                _updateQueue.Enqueue(updateAction);
            }
        }

        private void ProcessBatchUpdates(object state)
        {
            //如果批处理器或者控件被释放了 则返回
            if (_disposed || _control.IsDisposed) return;
            //创建临时列表收集所有待处理的更新
            var updates = new List<UIUpdateAction>();
            while (_updateQueue.TryDequeue(out var update))
            {
                updates.Add(update);
            }

            if (updates.Count > 0)
            {
                try
                {
                    _control.BeginInvoke(new Action(() =>
                    {
                        if (_control.IsDisposed) return;
                        foreach (var update in updates)
                        {
                            try
                            {
                                update.Execute();
                            }
                            catch (Exception ex)
                            {
                                //记录单个更新失败，但继续处理其他更新
                                System.Diagnostics.Debug.WriteLine($"UI更新失败:{ex.Message}");
                            }
                        }
                    }));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"批量UI更新失败:{ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _batchTimer?.Dispose();
                _disposed = true;
            }
        }
    }

    public abstract class UIUpdateAction
    {
        public abstract void Execute();
    }

    // 具体的UI更新动作类
    public class StatusUpdateAction : UIUpdateAction
    {
        private readonly Action _updateAction;

        public StatusUpdateAction(Action updateAction)
        {
            _updateAction = updateAction;
        }

        public override void Execute()
        {
            _updateAction?.Invoke();
        }
    }
}

    public class DeviceAwareUpdateAction : UIUpdateAction
    {
        private readonly Action _updateAction;
        private readonly string _deviceId;
        private readonly Func<string, bool> _isDeviceVisible;

        public DeviceAwareUpdateAction(Action updateAction, string deviceId, Func<string, bool> isDeviceVisible)
        {
            _updateAction = updateAction;
            _deviceId = deviceId;
            _isDeviceVisible = isDeviceVisible;
        }

        public override void Execute()
        {
            // 只有设备可见时才执行更新
            if (string.IsNullOrEmpty(_deviceId) || _isDeviceVisible(_deviceId))
            {
                _updateAction?.Invoke();
            }
        }
    }
