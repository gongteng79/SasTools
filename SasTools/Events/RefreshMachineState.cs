using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public enum MachineStatusType { Normal, Forward, Reverse, Waiting, Error, Idle };
    public class RefreshMachineState : IEvent
    {
        // 添加设备ID属性
        public string DeviceId { get; set; }

        public RefreshMachineState(string deviceId, string message, string status, MachineStatusType statusType = MachineStatusType.Normal)
        {
            DeviceId = deviceId;
            Status = status;
            Message = message;
            StatusType = statusType;
        }

        public string Status { get; set; }
        public string Message { get; set; }
        public MachineStatusType StatusType { get; set; }
        public DateTime TimeStamp => DateTime.Now;
    }
}
