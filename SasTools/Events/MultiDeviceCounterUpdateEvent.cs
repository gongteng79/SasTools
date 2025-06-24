using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class MultiDeviceCounterUpdateEvent : IEvent
    {
        public string DeviceId { get; }
        public int TotalCycles { get; }
        public int SuccessfulCycles { get; }
        public int FailedCycles { get; }
        public DateTime TimeStamp { get; }

        public MultiDeviceCounterUpdateEvent(string deviceId, int totalCycles, int successfulCycles, int failedCycles)
        {
            DeviceId = deviceId;
            TotalCycles = totalCycles;
            SuccessfulCycles = successfulCycles;
            FailedCycles = failedCycles;
            TimeStamp = DateTime.Now;
        }
    }
}