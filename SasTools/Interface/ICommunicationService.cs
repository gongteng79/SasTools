using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    // 通信服务接口，提供设备连接与通信功能
    public interface ICommunicationService
    {
        // 当前连接状态
        bool IsConnected { get; }


        // 连接设备
        Task<bool> ConnectAsync(string host = null, int port = 0);

        // 断开设备连接
        Task<bool> DisconnectAsync();

        // 发送字符串消息
        Task<string> SendMessageAsync(string message);

        // 发送二进制数据
        Task<string> SendMessageAsync(byte[] data);

        // 消息接收事件
        event EventHandler<string> MessageReceived;

        // 连接状态变化事件
        event EventHandler<bool> ConnectionStatusChanged;
    }
}
