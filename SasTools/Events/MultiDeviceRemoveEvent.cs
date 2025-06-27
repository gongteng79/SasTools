using System;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class MultiDeviceRemoveEvent : IEvent
    {
        public string DeviceId { get; set; }
        public DateTime TimeStamp { get; set; }

        public MultiDeviceRemoveEvent(string deviceId)
        {
            DeviceId = deviceId;
            TimeStamp = DateTime.Now;
        }
    }
}
