using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using SasTools.Common;
using SasTools.Domain;
using SasTools.Interface;
using SasTools.Models.Communication;
using System.Threading;

namespace SasTools.Services
{
    //通信服务实现类，使用依赖注入管理底层通信组件
    public class CommunicationService : ICommunicationService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(CommunicationService));
        private ICommunication _communication;
        private readonly string _defaultHost;
        private readonly int _defaultPort;
        private bool _isConnected = false;
        private Timer _heartbeatTimer;
        private readonly int _heartbeatInterval; // 心跳间隔时间,单位:毫秒

        // 消息接收事件
        public event EventHandler<string> MessageReceived;

        // 连接状态变化事件
        public event EventHandler<bool> ConnectionStatusChanged;

        //当前连接状态
        public bool IsConnected => _isConnected;


        // 构造函数，依赖注入通信参数
        public CommunicationService(string defaultHost = "192.168.2.12", int defaultPort = 6062)
        {
            _defaultHost = defaultHost;
            _defaultPort = defaultPort;
        }


        //连接设备
        public async Task<bool> ConnectAsync(string host = null, int port = 0)
        {
            try
            {
                string connectionHost = host ?? _defaultHost;
                int connectionPort = port > 0 ? port : _defaultPort;

                _logger.Info($"正在连接设备 {connectionHost}:{connectionPort}");
                _communication = CommunicationFactory.CreateTcpCommunication(connectionHost, connectionPort);

                // 注册数据接收事件
                if (_communication is CommunicationBase communicationBase)
                {
                    communicationBase.DataReceived += OnDataReceived;
                }

                var result = await _communication.ConnectAsync();
                _isConnected = result;

                _logger.Info($"设备 {connectionHost}:{connectionPort} 连接结果: {(result ? "成功" : "失败")}");
                OnConnectionStatusChanged(result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"连接设备时发生错误: {ex.Message}", ex);
                _isConnected = false;
                OnConnectionStatusChanged(false);
                return false;
            }
        }

        // 断开设备连接
        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_communication == null)
                {
                    _isConnected = false;
                    OnConnectionStatusChanged(false);
                    return true;
                }

                // 注销数据接收事件
                if (_communication is CommunicationBase communicationBase)
                {
                    communicationBase.DataReceived -= OnDataReceived;
                }

                var result = await _communication.DisconnectAsync();
                _isConnected = !result;

                _logger.Info($"设备断开连接结果: {(result ? "成功" : "失败")}");
                OnConnectionStatusChanged(!result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"断开设备连接时发生错误: {ex.Message}", ex);
                return false;
            }
        }

        // 发送字符串消息
        public async Task<string> SendMessageAsync(string message)
        {
            if (_communication == null || !IsConnected)
            {
                _logger.Warn("尝试在未连接状态下发送消息");
                return null;
            }

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                _logger.Debug($"发送消息: {message}");
                var response = await _communication.SendAsync(data);
                OnMessageReceived(response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error($"发送消息时发生错误: {ex.Message}", ex);
                return null;
            }
        }

        // 发送二进制数据
        public async Task<string> SendMessageAsync(byte[] data)
        {
            if (_communication == null || !IsConnected)
            {
                _logger.Warn("尝试在未连接状态下发送数据");
                return null;
            }

            try
            {
                _logger.Debug($"发送二进制数据，长度: {data.Length} 字节");
                var response = await _communication.SendAsync(data);
                OnMessageReceived(response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error($"发送数据时发生错误: {ex.Message}", ex);
                return null;
            }
        }

        //发送对象消息(序列化为JSON)
        public async Task<T> SendObjectAsycn<T, R>(R requestObj) where T : class
        {
            if (_communication == null || !IsConnected)
            {
                _logger.Warn("尝试在未连接状态下发送消息");
                return null;
            }
            try
            {
                //序列化对象为JSON字符串
                string jsonRequest = JsonMessageCodec.Serialize(requestObj);
                _logger.Debug($"发送JSON消息:{jsonRequest}");

                //发送JSON消息
                var jsonResponse = await SendMessageAsync(jsonRequest);

                //反序列化响应
                if (jsonRequest != null)
                {
                    return JsonMessageCodec.Deserialize<T>(jsonRequest);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"发送对象消息时发生错误: {ex.Message}", ex);
                return null;
            }
        }

        //数据接收事件处理
        private void OnDataReceived(object sender, byte[] data)
        {
            try
            {
                string message = Encoding.UTF8.GetString(data);
                _logger.Debug($"接收到数据: {message}");
                OnMessageReceived(message);
            }
            catch (Exception ex)
            {
                _logger.Error($"处理接收数据时发生错误: {ex.Message}", ex);
            }
        }

        // 触发消息接收事件
        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        // 触发连接状态变化事件
        protected virtual void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }
    }
}
