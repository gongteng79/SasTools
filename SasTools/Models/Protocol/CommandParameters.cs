using System.Collections.Generic;

namespace SasTools.Models.Protocol
{
    public class CommandParameters
    {
        public int? Velocity { get; set; }
        public int? Time { get; set; }
        public double? Torque { get; set; }
        public double? Angle { get; set; }
        public int SlaveId { get; set; } = 1;
        public Dictionary<string, object> ExtendedParams { get; set; } = new Dictionary<string, object>();
    }
}