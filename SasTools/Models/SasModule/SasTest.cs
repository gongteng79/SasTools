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
            // 将消息对象序列化为 JSON 字符串
            string sendData = JsonConvert.SerializeObject(data);

            // 将 JSON 字符串转换为字节数组，准备发送
            byte[] sendBytes = Encoding.ASCII.GetBytes(sendData);

            var result = _tcpCommunication.SendAsync(sendBytes);

            this._eventBus.Publish(new SendDataEvent(sendData, result.Result.ToJsonString()));

            // 返回 JSON 字符串（或根据需要返回字节数组）
            return result.Result.ToJsonString();
        }
    }
}
