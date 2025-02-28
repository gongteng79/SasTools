using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Models.Communication
{
    public class SerialCommunication : CommunicationBase
    {
        private SerialPort _serialPort;
        private readonly string _portName;
        private readonly int _baudRate;
        public override bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialCommunication(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public override Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<string> SendAsync(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
