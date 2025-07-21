namespace SasTools.Models.Protocol
{
    public abstract class ProtocolConfig
    {
        public abstract string ConnectionString { get; }
        public abstract ProtocolType ProtocolType { get; }
    }

    public class JsonProtocolConfig : ProtocolConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public override string ConnectionString => $"{Host}:{Port}";
        public override ProtocolType ProtocolType => ProtocolType.Json;
    }

    public class ModbusProtocolConfig : ProtocolConfig
    {
        public string Host { get; set; } = "192.168.1.12";
        public int Port { get; set; } = 1502;
        public byte SlaveId { get; set; } = 1;

        public override string ConnectionString => $"modbus://{Host}:{Port}/{SlaveId}";
        public override ProtocolType ProtocolType => ProtocolType.ModbusTcp;
    }
}
