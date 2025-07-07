using HslCommunication;
using HslCommunication.Enthernet;
using Newtonsoft.Json;
using SasTools.Common;
using SasTools.Domain;
using SasTools.Events;
using SasTools.Interface;
using SasTools.Models.Communication;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wp.FzWater.Mqtt;
using WpFramework.EventBus;

namespace SasTools.Models.SasModule
{
    public class SasDevice: IDevice
    {
        private ICommunication _tcpCommunication;
        private IEventBus _eventBus;
        private ICommunicationService _communicationService;

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

        public bool ConnectServer()
        {
            if (_communicationService != null)
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
            if (_communicationService != null)
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
            if (_communicationService != null)
            {
                result = _communicationService.SendMessageAsync(sendData).Result;
            }
            else
            {
                result = _tcpCommunication.SendAsync(sendData).Result;
            }

            return result;
        }



        public string ExecuteCommand(SasCommandType commandType)
        {
            var response = ExcuteCommand(commandType);
            return response;
        }

        public Task<string> ExecuteCommandAsync(SasCommandType commandType)
        {
            var response = ExcuteCommand(commandType);
            return Task.FromResult(response);
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
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
