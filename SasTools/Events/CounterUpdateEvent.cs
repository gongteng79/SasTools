using System;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class CounterUpdateEvent : IEvent
    {
        // 添加设备ID属性
        public string DeviceId { get; set; }

        public int TotalCycles { get; set; }
        public int SuccessfulCycles { get; set; }
        public int FailedCycles { get; set; }
        public DateTime TimeStamp => DateTime.Now;

        public CounterUpdateEvent(string deviceId, int totalCycles, int successfulCycles, int failedCycles)
        {
            DeviceId = deviceId;
            TotalCycles = totalCycles;
            SuccessfulCycles = successfulCycles;
            FailedCycles = failedCycles;
        }
    }
}
