using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    public interface ICommunication
    {
        bool IsConnected { get; }
        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();
        Task<string> SendAsync(byte[] data);
        event EventHandler<byte[]> DataReceived;
    }

}
