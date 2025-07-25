using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;

namespace SasTools.Infrastructure
{
    //事件订阅管理器 - 自动跟踪和清理事件订阅
    public class EventSubscriptionManager : IDisposable
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(EventSubscriptionManager));
        private readonly ConcurrentDictionary<string, List<IDisposable>> _subscriptions;
        private bool _disposed = false;

        public EventSubscriptionManager()
        {
            _subscriptions = new ConcurrentDictionary<string, List<IDisposable>>();
        }

        public void RegisterSubscription(string ownerId, IDisposable subscription)
        {
            _subscriptions.AddOrUpdate(ownerId,
                new List<IDisposable> { subscription },
                (key, existing) => { existing.Add(subscription); return existing; });
        }

        public void UnsubscribeAll(string ownerId)
        {
            if (_subscriptions.TryRemove(ownerId, out var subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    subscription?.Dispose();
                }
                _logger.Info($"已清理 {ownerId} 的所有事件订阅");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var kvp in _subscriptions)
                {
                    UnsubscribeAll(kvp.Key);
                }
                _subscriptions.Clear();
                _disposed = true;
            }
        }
    }
}
