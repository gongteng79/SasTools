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

namespace SasTools.Models.SasModule
{
    public class SasTest: ISasTest
    {
        private ICommunication _tcpCommunication;
        private IEventBus _eventBus;

        public SasTest(ICommunication tcpCommunication, IEventBus eventBus)
        {
           this._tcpCommunication = tcpCommunication;
           this._eventBus = eventBus;
        }

        public bool ConnectServer()
        {
            var result = this._tcpCommunication.ConnectAsync();

            return result.Result;
        }

        public bool DisconnectServer()
        {
            var result = this._tcpCommunication.DisconnectAsync();

            return result.Result;
        }

        public string ReadData(RequestData data)
        {
            string jsonString = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            byte[] sendData = Encoding.ASCII.GetBytes(jsonString);

            var result = _tcpCommunication.SendAsync(sendData);

            this._eventBus.Publish(new SendDataEvent(jsonString, result.Result));

            // 返回 JSON 字符串（或根据需要返回字节数组）
            return result.Result;
        }
    }
}
