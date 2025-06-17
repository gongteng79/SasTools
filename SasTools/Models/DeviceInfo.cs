using System;

namespace SasTools.Models
{
    public class DeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? LastConnectedTime { get; set; }
        public string Status { get; set; } = "未连接";

        public DeviceInfo()
        {
            Id = Guid.NewGuid().ToString();
        }

        public DeviceInfo(string name, string host, int port) : this()
        {
            Name = name;
            Host = host;
            Port = port;
        }
    }
}
