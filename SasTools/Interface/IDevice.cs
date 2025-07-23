using SasTools.Domain;
using SasTools.Models.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    public interface IDevice
    {
        bool ConnectServer();

        bool DisconnectServer();

        string ReadData(RequestData data);

        string ExecuteCommand(SasCommandType commandType);

        Task<string> ExecuteCommandAsync(SasCommandType commandType);

        string ExecuteCommandWithParameters(SasCommandType commandType, int? velocity = null, int? time = null);

        Task<string> ExecuteCommandWithParametersAsync(SasCommandType commandType, int? velocity = null, int? time = null);

        //添加协议处理器访问方法
        IProtocolHandler GetProtocolHandler();

        bool HasProtocolHandler();
    }
}
