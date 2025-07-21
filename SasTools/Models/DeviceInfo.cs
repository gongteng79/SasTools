using SasTools.Models.Protocol;
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
        public ProtocolType ProtocolType { get; set; } = ProtocolType.Json;
        public ProtocolConfig ProtocolConfig { get; set; }
        public void SetJsonProtocol(string host, int port)
        {
            ProtocolType = ProtocolType.Json;
            ProtocolConfig = new JsonProtocolConfig { Host = host, Port = port };
        }
        public void SetModbusProtocol(string host, int port, byte slaveId = 1)
        {
            ProtocolType = ProtocolType.ModbusTcp;
            ProtocolConfig = new ModbusProtocolConfig { Host = host, Port = port, SlaveId = slaveId };
        }

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
