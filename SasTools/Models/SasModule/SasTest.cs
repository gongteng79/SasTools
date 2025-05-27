using HslCommunication;
using HslCommunication.Enthernet;
using Newtonsoft.Json;
using SasTools.Domain;
using SasTools.Events;
using SasTools.Interface;
using SasTools.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wp.FzWater.Mqtt;
using WpFramework.EventBus;
using static SasTools.Events.SendDataEvent;

namespace SasTools.Models.SasModule
{
    public class SasTest: ISasTest
    {
        private ICommunication _tcpCommunication;
        private IEventBus _eventBus;
        private ICommunicationService _communicationService;

        public SasTest(ICommunication tcpCommunication, IEventBus eventBus)
        {
           this._tcpCommunication = tcpCommunication;
           this._eventBus = eventBus;
        }

        // 为依赖注入提供的替代构造函数
        public SasTest(ICommunicationService communicationService, IEventBus eventBus)
        {
           this._communicationService = communicationService;
           this._eventBus = eventBus;
        }

        public bool ConnectServer()
        {
            if (_communicationService != null)
            {
                var result = _communicationService.ConnectAsync();
                return result.Result;
            }
            else
            {
                var result = this._tcpCommunication.ConnectAsync();
                return result.Result;
            }
        }

        public bool DisconnectServer()
        {
            if (_communicationService != null)
            {
                var result = _communicationService.DisconnectAsync();
                return result.Result;
            }
            else
            {
                var result = this._tcpCommunication.DisconnectAsync();
                return result.Result;
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

            this._eventBus.Publish(EventFactory.CreateSendDataEvent(jsonString, result));


            // 返回 JSON 字符串
            return result;
        }
    }
}
