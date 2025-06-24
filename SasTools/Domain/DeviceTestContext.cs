using SasTools.Events;
using SasTools.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Domain
{
    public class DeviceTestContext
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public IDevice Device { get; set; }
        public TestStateMachine StateMachine { get; set; }
        public DataTable DataTable { get; set; }

        public bool IsTestRunning { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int TotalCycles { get; set; }
        public int SuccessfulCycles { get; set; }
        public int FailureCycles { get; set; }
        public MachineStatusType CurrentStatus { get; set; }

        public DeviceTestContext(string deviceId, string deviceName, IDevice device, FatigueParams parameters, IEventBus eventBus)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            Device = device;
            StateMachine = new TestStateMachine(deviceId,device,parameters,eventBus);
            DataTable = CreateDataTable();
            IsTestRunning = false;
            LastUpdateTime = DateTime.Now;
            CurrentStatus = MachineStatusType.Idle;
        }

        private DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("Time", typeof(string));
            table.Columns.Add("states", typeof(string));
            table.Columns.Add("Message", typeof(string));
            table.Columns.Add("StateType", typeof(string));
            return table;
        }
    }
}