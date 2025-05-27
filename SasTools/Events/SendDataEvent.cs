using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class SendDataEvent : IEvent
    {
        public SendDataEvent(string sendData, string recieveData)
        {
            SendData = sendData;
            RecieveData = recieveData;
        }

        public class EventFactory
        {
            public static SendDataEvent CreateSendDataEvent(string sendData, string recieveData)
            {
                return new SendDataEvent(sendData, recieveData);
            }
        }


        public string SendData { get; set; }

        public string RecieveData { get; set; }

        public DateTime TimeStamp => DateTime.Now;
    }
}
