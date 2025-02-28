using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasTools.Interface;

namespace SasTools.Models.Communication
{
    public abstract class CommunicationBase : ICommunication
    {
        public abstract bool IsConnected { get; }
        public event EventHandler<byte[]> DataReceived;

        public abstract Task<bool> ConnectAsync();
        public abstract Task<bool> DisconnectAsync();
        public abstract Task<string> SendAsync(byte[] data);

        protected void OnDataReceived(byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }
    }
}
