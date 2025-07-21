using log4net;
using Newtonsoft.Json;
using SasTools.Domain;
using SasTools.Interface;
using SasTools.Models.Protocol;
using SasTools.Models.SasModule;
using SasTools.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SasTools.Models.Communication
{
    public class JsonProtocolHandler : IProtocolHandler
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(JsonProtocolHandler));
        private ICommunicationService _communicationService;
        private bool _disposed = false;

        public ProtocolType ProtocolType => ProtocolType.Json;
        public bool IsConnected => _communicationService?.IsConnected ?? false;

        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<string> MessageReceived;

        public async Task<bool> ConnectAsync(ProtocolConfig config)
        {
            if (!(config is JsonProtocolConfig jsonConfig))
                throw new ArgumentException("Invalid config type for JSON protocol");

            try
            {
                _communicationService = new CommunicationService(jsonConfig.Host, jsonConfig.Port);
                _communicationService.ConnectionStatusChanged += OnConnectionStatusChanged;
                _communicationService.MessageReceived += OnMessageReceived;

                var result = await _communicationService.ConnectAsync();
                _logger.Info($"JSON协议连接结果: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"JSON协议连接失败: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            if (_communicationService != null)
            {
                return await _communicationService.DisconnectAsync();
            }
            return true;
        }

        public async Task<DeviceResponse> ExecuteCommandAsync(SasCommandType commandType, CommandParameters parameters = null)
        {
            try
            {
                // 使用现有的SasDevice逻辑
                var device = new SasDevice(_communicationService, null);

                string response;
                if (parameters != null && commandType == SasCommandType.Reverse)
                {
                    response = device.ExecuteCommandWithParameters(commandType, parameters.Velocity, parameters.Time);
                }
                else
                {
                    response = device.ExecuteCommand(commandType);
                }

                return new DeviceResponse
                {
                    Success = !string.IsNullOrEmpty(response),
                    Message = response,
                    Data = new Dictionary<string, object> { ["raw_response"] = response }
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"JSON协议命令执行失败: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<DeviceResponse> ReadDataAsync()
        {
            try
            {
                var device = new SasDevice(_communicationService, null);
                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);

                if (string.IsNullOrEmpty(jsonMsg))
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = "设备返回空数据"
                    };
                }

                var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);
                if (response != null && response.reply == 203 &&
                    response.state != null && response.result != null)
                {
                    return new DeviceResponse
                    {
                        Success = true,
                        State = (int)response.state,
                        Result = (int)response.result,
                        Reply = (int)response.reply,
                        Data = new Dictionary<string, object> { ["raw_json"] = jsonMsg }
                    };
                }

                return new DeviceResponse
                {
                    Success = false,
                    Message = "JSON响应格式错误",
                    Data = new Dictionary<string, object> { ["raw_json"] = jsonMsg }
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"JSON数据读取失败: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private void OnConnectionStatusChanged(object sender, bool connected)
        {
            ConnectionStatusChanged?.Invoke(this, connected);
        }

        private void OnMessageReceived(object sender, string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _communicationService?.DisconnectAsync();
                _communicationService = null;
                _disposed = true;
            }
        }
    }
}
