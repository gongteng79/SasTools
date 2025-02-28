using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasTools.Interface;

namespace SasTools.Services
{
    public class CommunicationService : ICommunicationService
    {
        public event EventHandler<string> MessageReceived;

        public Task SendMessageAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
