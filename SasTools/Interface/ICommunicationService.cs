using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    public interface ICommunicationService
    {
        Task<bool> StartAsync();
        Task StopAsync();
        Task SendMessageAsync(string message);
        event EventHandler<string> MessageReceived;
    }
}
