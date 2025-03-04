using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HslCommunication;
using HslCommunication.Core.Device;
using HslCommunication.Core.Net;
using log4net;

namespace SasTools.Models.Communication
{
    public class TcpCommunication : CommunicationBase
    {
        private TcpNetCommunication _client;
        private readonly string _host;
        private readonly int _port;
        private readonly ILog _logger = LogManager.GetLogger("TcpNetCommunication");

        public TcpCommunication(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public override bool IsConnected => throw new NotImplementedException();

        public override Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpNetCommunication(_host, _port);

                var result = _client.ConnectServer();

                _logger.Info(result.Message.ToString());

                return Task.FromResult(result.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }

        }

        public override Task<bool> DisconnectAsync()
        {
            try
            {
                var result = _client.ConnectClose();

                _logger.Info(result.Message.ToString());

                return Task.FromResult(result.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        public override Task<string> SendAsync(byte[] data)
        {
            try
            {
                var response = _client.ReadFromCoreServer(data);

                string receivedJsonString = Encoding.ASCII.GetString(response.Content);
                return Task.FromResult(receivedJsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
