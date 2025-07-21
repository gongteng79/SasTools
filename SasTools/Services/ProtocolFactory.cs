using SasTools.Interface;
using SasTools.Models.Communication;
using SasTools.Models.Protocol;
using System;

namespace SasTools.Services
{
    public static class ProtocolFactory
    {
        public static IProtocolHandler CreateProtocolHandler(ProtocolType protocolType)
        {
            switch (protocolType)
            {
                case ProtocolType.Json:
                    return new JsonProtocolHandler();
                case ProtocolType.ModbusTcp:
                    return new ModbusProtocolHandler();
                default:
                    throw new NotSupportedException($"不支持的协议类型: {protocolType}");
            }
        }

        public static ProtocolConfig CreateProtocolConfig(ProtocolType protocolType, string host, int port)
        {
            switch (protocolType)
            {
                case ProtocolType.Json:
                    return new JsonProtocolConfig { Host = host, Port = port };
                case ProtocolType.ModbusTcp:
                    return new ModbusProtocolConfig { Host = host, Port = port };
                default:
                    throw new NotSupportedException($"不支持的协议类型: {protocolType}");
            }
        }
    }
}
