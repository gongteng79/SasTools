using HslCommunication;
using HslCommunication.Enthernet;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Domain;
using SasTools.Events;
using SasTools.Interface;
using SasTools.Models.Communication;
using SasTools.Models.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wp.FzWater.Mqtt;
using WpFramework.EventBus;

namespace SasTools.Models.SasModule
{
    public class SasDevice : IDevice
    {
        private readonly ICommunication _tcpCommunication;
        private readonly IEventBus _eventBus;
        private readonly ICommunicationService _communicationService;
        private readonly IProtocolHandler _protocolHandler;
        public SasDevice(ICommunication tcpCommunication, IEventBus eventBus)
        {
            this._tcpCommunication = tcpCommunication;
            this._eventBus = eventBus;
        }

        public SasDevice(ICommunicationService communicationService, IEventBus eventBus)
        {
            this._communicationService = communicationService;
            this._eventBus = eventBus;
        }

        public SasDevice(IProtocolHandler protocolHandler, IEventBus eventBus)
        {
            _protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }


        public IProtocolHandler GetProtocolHandler()
        {
            return _protocolHandler;
        }

        public bool HasProtocolHandler()
        {
            return _protocolHandler != null;
        }
        public bool ConnectServer()
        {
            if (_protocolHandler != null)
            {
                return _protocolHandler.IsConnected;
            }
            else if (_communicationService != null)
            {
                return _communicationService.ConnectAsync().Result;
            }
            else
            {
                return this._tcpCommunication.ConnectAsync().Result;
            }
        }

        public bool DisconnectServer()
        {
            if (_protocolHandler != null)
            {
                return _protocolHandler.DisconnectAsync().Result;
            }
            else if (_communicationService != null)
            {
                return _communicationService.DisconnectAsync().Result;
            }
            else
            {
                return this._tcpCommunication.DisconnectAsync().Result;
            }
        }

        public string ReadData(RequestData data)
        {
            string jsonString = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            byte[] sendData = Encoding.ASCII.GetBytes(jsonString);

            string result;

            // 优先使用协议处理器
            if (_protocolHandler != null)
            {
                var task = _protocolHandler.ReadDataAsync();
                task.Wait();
                var response = task.Result;
                result = response.Success ? response.Message : $"Error: {response.Message}";
            }
            else if (_communicationService != null)
            {
                result = _communicationService.SendMessageAsync(sendData).Result;
            }
            else if (_tcpCommunication != null)
            {
                result = _tcpCommunication.SendAsync(sendData).Result;
            }
            else
            {
                throw new InvalidOperationException("没有可用的通信方式");
            }

            return result;
        }

        public string ExecuteCommand(SasCommandType commandType)
        {
            // 如果使用协议处理器，优先使用协议处理器
            if (_protocolHandler != null)
            {
                var protocolResponse = _protocolHandler.ExecuteCommandAsync(commandType).Result;
                return protocolResponse.Success ? protocolResponse.Message : $"Error: {protocolResponse.Message}";
            }

            var legacyResponse = ExcuteCommand(commandType);
            return legacyResponse;
        }

        public Task<string> ExecuteCommandAsync(SasCommandType commandType)
        {
            if (_protocolHandler != null)
            {
                return ExecuteCommandAsync(commandType, null, null);
            }

            var legacyResponse = ExcuteCommand(commandType);
            return Task.FromResult(legacyResponse);
        }

        public async Task<string> ExecuteCommandAsync(SasCommandType commandType, int? velocity = null, int? time = null)
        {
            if (_protocolHandler != null)
            {
                var parameters = new CommandParameters
                {
                    Velocity = velocity,
                    Time = time
                };

                var response = await _protocolHandler.ExecuteCommandAsync(commandType, parameters);
                return response.Success ? response.Message : $"Error: {response.Message}";
            }

            return ExecuteCommandWithParameters(commandType, velocity, time);
        }

        public async Task<DeviceResponse> GetDeviceStatusAsync()
        {
            if (_protocolHandler != null)
            {
                return await _protocolHandler.ReadDataAsync();
            }

            try
            {
                string jsonMsg = ExecuteCommand(SasCommandType.InputScrewData);

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
                return new DeviceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private string ExcuteCommand(SasCommandType commandType)
        {
            try
            {
                RequestData requestData;
                RequestParameter parameters = null;

                switch (commandType)
                {
                    case SasCommandType.Subscribe:
                        parameters = new RequestParameter
                        {
                            Request = 101,
                            Sequence = 123,
                            KeepAlive = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.Subcribe, parameters);
                        break;

                    case SasCommandType.InputScrewData:
                        parameters = new RequestParameter
                        {
                            Request = 102,
                            Sequence = 123,
                            SlaveId = 1,
                            Screw = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.InputScrewData, parameters);
                        break;
                    case SasCommandType.Forward:
                        parameters = new RequestParameter
                        {
                            Request = 116,
                            SlaveId = 1,
                            ScrewId = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.LockScrewAction, parameters);
                        break;
                    case SasCommandType.Reverse:
                        parameters = new RequestParameter
                        {
                            Request = 115,
                            SlaveId = 1,
                            Torque = 0, //最大扭矩
                            Velocity = 500,
                            Time = 1000,//反转转动的时间
                            Angle = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.RemoveScrewAction, parameters);
                        break;
                    case SasCommandType.Stop:
                        parameters = new RequestParameter
                        {
                            Request = 118,
                            SlaveId = 1,
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.Stop, parameters);
                        break;
                    case SasCommandType.ClearTightenInfo:
                        parameters = new RequestParameter
                        {
                            Request = 125,
                            SlaveId = 1,
                            TightenClear = 1
                        };              
                        requestData = DataFactory.CreateRequestCommand(FunctionType.TightenInfoControl, parameters);
                        break;
                    case SasCommandType.StatusQuery:
                        parameters = new RequestParameter
                        {
                            Request = 119,
                            SlaveId = 1,
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.StatusQuery, parameters);
                        break;
                    default:
                        throw new ArgumentException($"未知的命令类型: {commandType}");
                }

                if (parameters == null)
                {
                    System.Windows.Forms.MessageBox.Show("parameters 为空！");
                }

                string response = this.ReadData(requestData);

                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 添加支持动态参数的ExecuteCommand重载方法
        public string ExecuteCommandWithParameters(SasCommandType commandType, int? velocity = null, int? time = null)
        {
            // 如果使用协议处理器，优先使用协议处理器
            if (_protocolHandler != null)
            {
                var parameters = new CommandParameters
                {
                    Velocity = velocity,
                    Time = time
                };

                var protocolResponse = _protocolHandler.ExecuteCommandAsync(commandType, parameters).Result;

                return protocolResponse.Success ? "OK" : $"Error: {protocolResponse.Message}";
            }

            var response = ExcuteCommandWithParameters(commandType, velocity, time);
            return response;
        }

        public Task<string> ExecuteCommandWithParametersAsync(SasCommandType commandType, int? velocity = null, int? time = null)
        {
            // 如果使用协议处理器，优先使用协议处理器
            if (_protocolHandler != null)
            {
                return ExecuteCommandAsync(commandType, velocity, time);
            }

            var legacyResponse = ExcuteCommandWithParameters(commandType, velocity, time);
            return Task.FromResult(legacyResponse);
        }

        private string ExcuteCommandWithParameters(SasCommandType commandType, int? velocity = null, int? time = null)
        {
            try
            {
                RequestData requestData;
                RequestParameter parameters = null;

                switch (commandType)
                {
                    case SasCommandType.Reverse:
                        parameters = new RequestParameter
                        {
                            Request = 115,
                            SlaveId = 1,
                            Torque = 0, //最大扭矩
                            Velocity = velocity ?? 500,
                            Time = time ?? 1000,
                            Angle = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.RemoveScrewAction, parameters);
                        break;

                    default:
                        return ExcuteCommand(commandType);
                }

                if (parameters == null)
                {
                    System.Windows.Forms.MessageBox.Show("parameters 为空！");
                }

                string response = this.ReadData(requestData);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}