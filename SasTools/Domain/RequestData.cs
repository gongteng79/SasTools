using AntdUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    public class RequestData
    {
        public int Request { get; set; }
        public int SlaveId { get; set; }
        public int? Sequence { get; set; }
        public int? Screw { get; set; }
        public int? Velocity { get; set; }
        public int? Time { get; set; }
        public double? Angle { get; set; }
        public double? Torque { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ScrewId { get; set; }
        public double? TorqueCompensation { get; set; }
        public int? TorqueValidTime { get; set; }
        public int? TorqueFilterTime { get; set; }
        public int? ScrewCount { get; set; }
        public double? VelocityTarget { get; set; }
        public double? VelocityMin { get; set; }
        public double? VelocityMax { get; set; }
        public double? TorqueTarget { get; set; }
        public double? TorqueMin { get; set; }
        public double? TorqueMax { get; set; }
        public double? AngleTarget { get; set; }
        public double? AngleMin { get; set; }
        public double? AngleMax { get; set; }
        public int? StepCount { get; set; }
        public int? StepValidStart { get; set; }
        public Step[] Steps { get; set; }
        public int? CurrentPercent { get; set; }
        public int? Ctrl { get; set; }
        public int? PowerEnable { get; set; }
        public int? TightenClear { get; set; }
        public int? BarCode { get; set; }
    }

}
