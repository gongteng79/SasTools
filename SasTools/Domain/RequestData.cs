using AntdUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    public class RequestData
    {
        [JsonProperty("slave id")]
        public int slave_id { get; set; }

        [JsonProperty("product id")]
        public int? product_id { get; set; }

        [JsonProperty("screw id")]
        public int? screw_id { get; set; }

        [JsonProperty("keep alive")]
        public int? keep_alive { get; set; }
        [JsonProperty("cache clear")]
        public int? cache_clear { get; set; }
        public int request { get; set; }
        public int? sequence { get; set; }
        public int? screw { get; set; }
        public int? velocity { get; set; }
        public int? time { get; set; }
        public double? angle { get; set; }
        public double? torque { get; set; }
        public string product_Name { get; set; }
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

    public class RequestParameter
    {
        public int SlaveId { get; set; } = 1;
        public int Request { get; set; }
        public int Sequence { get; set; } = 0;
        public int? BarCode { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public int? Screw { get; set; }
        public double? Torque { get; set; }
        public int? Velocity { get; set; }
        public int? Time { get; set; }
        public double? Angle { get; set; }
        public int? KeepAlive { get; set; }
        public int? CacheClear { get; set; }
        public int? CurrentPercent { get; set; }
        public int? PowerEnable { get; set; }
        public int? TightenClear { get; set; }
        public int? Ctrl { get; set; }
    }

}
