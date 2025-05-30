using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasTools.Interface;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class DeviceCreateEvent : IEvent
    {
        public DeviceCreateEvent(IDevice sasDevice)
        {
            SasDevice = sasDevice;
        }

        public IDevice SasDevice { get; set; }

        public DateTime TimeStamp => DateTime.Now;
    }
}
