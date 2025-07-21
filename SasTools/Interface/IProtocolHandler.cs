using SasTools.Domain;
using SasTools.Models.Protocol;
using System;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    public interface IProtocolHandler : IDisposable
    {
        ProtocolType ProtocolType { get; }
        bool IsConnected { get; }

        Task<bool> ConnectAsync(ProtocolConfig config);
        Task<bool> DisconnectAsync();

        Task<DeviceResponse> ExecuteCommandAsync(SasCommandType commandType, CommandParameters parameters = null);
        Task<DeviceResponse> ReadDataAsync();

        event EventHandler<bool> ConnectionStatusChanged;
        event EventHandler<string> MessageReceived;
    }
}
