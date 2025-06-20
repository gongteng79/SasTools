using SasTools.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class MultiDeviceCreateEvent:IEvent
    {
        public string DeviceId { get; set; }
        public IDevice Device { get; set; }
        public DateTime TimeStamp { get; set; }

        public MultiDeviceCreateEvent(string deviceId, IDevice device)
        {
            DeviceId = deviceId;
            Device = device;
            TimeStamp = DateTime.Now;
        }
    }
}
