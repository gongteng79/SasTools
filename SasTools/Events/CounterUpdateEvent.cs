using System;
using WpFramework.EventBus;

namespace SasTools.Events
{
    public class CounterUpdateEvent : IEvent
    {
        //总循环次数  
        public int TotalCycles { get; private set; }
 
        //成功循环次数  
        public int SuccessfulCycles { get; private set; }

        //失败循环次数
        public int FailedCycles { get; private set; }

        //时间戳  
        public DateTime TimeStamp { get; private set; }

        
        public CounterUpdateEvent(int totalCycles, int successfulCycles,int failedCycles)
        {
            TotalCycles = totalCycles;
            SuccessfulCycles = successfulCycles;
            FailedCycles = failedCycles;
            TimeStamp = DateTime.UtcNow;
        }
    }
}
