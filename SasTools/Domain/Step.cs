using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    public class Step
    {
        public int Index { get; set; }
        public double Velocity { get; set; }
        public double Torque { get; set; }
        public double Angle { get; set; }
        public int Time { get; set; }
        public double VelocityFrom { get; set; }
        public double VelocityTo { get; set; }
        public int Dir { get; set; }
        public int Es { get; set; }
        public int[] OkIf { get; set; }
    }

}
