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
        public int request { get; set; }
        public int slaveId { get; set; }
        public int? sequence { get; set; }
        public int? screw { get; set; }
        public int? velocity { get; set; }
        public int? time { get; set; }
        public double? angle { get; set; }
        public double? torque { get; set; }
        public int? productId { get; set; }
        public string productName { get; set; }
        public int? screwId { get; set; }
        public double? torqueCompensation { get; set; }
        public int? torqueValidTime { get; set; }
        public int? torqueFilterTime { get; set; }
        public int? screwCount { get; set; }
        public double? velocityTarget { get; set; }
        public double? velocityMin { get; set; }
        public double? velocityMax { get; set; }
        public double? torqueTarget { get; set; }
        public double? torqueMin { get; set; }
        public double? torqueMax { get; set; }
        public double? angleTarget { get; set; }
        public double? angleMin { get; set; }
        public double? angleMax { get; set; }
        public int? stepCount { get; set; }
        public int? stepValidStart { get; set; }
        public Step[] steps { get; set; }
        public int? currentPercent { get; set; }
        public int? ctrl { get; set; }
        public int? powerEnable { get; set; }
        public int? tightenClear { get; set; }
        public int? barCode { get; set; }
    }

}
