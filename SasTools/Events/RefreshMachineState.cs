using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class RefreshMachineState : IEvent
    {
        public RefreshMachineState(string status, String message)
        {
            Status = status;
            Message = message;
        }

        public string Status { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp => DateTime.Now;
    }
}
