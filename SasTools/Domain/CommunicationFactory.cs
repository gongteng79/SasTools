using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasTools.Common;
using SasTools.Interface;
using SasTools.Models.Communication;

namespace SasTools.Domain
{
    public static class CommunicationFactory
    {
        public static ICommunication CreateTcpCommunication(string host, int port)
        {
            return new TcpCommunication(host, port);
        }

        public static ICommunication CreateSerialCommunication(string portName, int baudRate)
        {
            return new SerialCommunication(portName, baudRate);
        }
    }
}
