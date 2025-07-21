using System;
using System.Collections.Generic;

namespace SasTools.Models.Protocol
{
    public class DeviceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int State { get; set; }
        public int Result { get; set; }
        public int Reply { get; set; } = 203; // 兼容JSON格式
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
