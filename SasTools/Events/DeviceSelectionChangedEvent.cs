using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class DeviceSelectionChangedEvent : IEvent
    {
        public string SelectedDeviceId { get; }
        public string PreviousDeviceId { get; }
        public DateTime TimeStamp { get; }

        public DeviceSelectionChangedEvent(string selectedDeviceId, string previousDeviceId)
        {
            SelectedDeviceId = selectedDeviceId;
            PreviousDeviceId = previousDeviceId;
            TimeStamp = DateTime.Now;
        }
    }
}
