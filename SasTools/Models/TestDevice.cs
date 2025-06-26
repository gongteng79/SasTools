using SasTools.Domain;
using SasTools.Interface;
using System;
using System.Threading.Tasks;

namespace SasTools.Models
{
    /// <summary>
    /// 测试设备类，用于演示多设备功能
    /// </summary>
    public class TestDevice : IDevice
    {
        private readonly string _host;
        private readonly int _port;
        private bool _isConnected;

        public TestDevice(string host, int port)
        {
            _host = host;
            _port = port;
            _isConnected = false;
        }

        public bool ConnectServer()
        {
            // 模拟连接过程
            _isConnected = true;
            return true;
        }

        public bool DisconnectServer()
        {
            // 模拟断开连接
            _isConnected = false;
            return true;
        }

        public string ExecuteCommand(SasCommandType commandType)
        {
            // 模拟命令执行，返回模拟的JSON响应
            switch (commandType)
            {
                case SasCommandType.Subscribe:
                    return "{\"reply\":101,\"sequence\":123,\"result\":0}";
                
                case SasCommandType.InputScrewData:
                    return "{\"reply\":203,\"sequence\":123,\"state\":0,\"result\":0}";
                
                case SasCommandType.Forward:
                    return "{\"reply\":116,\"sequence\":123,\"result\":0}";
                
                case SasCommandType.Reverse:
                    return "{\"reply\":117,\"sequence\":123,\"result\":0}";
                
                case SasCommandType.Stop:
                    return "{\"reply\":118,\"sequence\":123,\"result\":0}";
                
                case SasCommandType.ClearTightenInfo:
                    return "{\"reply\":115,\"sequence\":123,\"result\":0}";
                
                case SasCommandType.StatusQuery:
                    return "{\"reply\":119,\"sequence\":123,\"state\":0,\"result\":0}";
                
                default:
                    return "{\"reply\":999,\"sequence\":123,\"result\":-1,\"error\":\"Unknown command\"}";
            }
        }

        public Task<string> ExecuteCommandAsync(SasCommandType commandType)
        {
            return Task.FromResult(ExecuteCommand(commandType));
        }

        public string ReadData(RequestData data)
        {
            // 模拟数据读取
            return "{\"reply\":200,\"sequence\":123,\"result\":0,\"data\":\"test\"}";
        }

        public override string ToString()
        {
            return $"TestDevice({_host}:{_port})";
        }
    }
}
