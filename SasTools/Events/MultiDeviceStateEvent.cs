using System;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class MultiDeviceStateEvent : IEvent
    {
        public string DeviceId { get; }
        public string Status { get; }
        public string Message { get; }
        public MachineStatusType StatusType { get; }
        public DateTime TimeStamp { get; }

        public MultiDeviceStateEvent(string deviceId, string status, string message, MachineStatusType statusType)
        {
            DeviceId = deviceId;
            Status = status;
            Message = message;
            StatusType = statusType;
            TimeStamp = DateTime.Now;
        }
    }
}
