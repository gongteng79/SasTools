using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    // 测试状态机上下文 - 存储状态机共享数据
    public class TestStateMachineContext
    {
        // 测试参数
        public TestParameters Parameters { get; set; }

        // 计数器
        public int TotalCycleCount { get; set; }
        public int SuccessCount { get; set; }

        // 时间戳
        public DateTime StateStartTime { get; set; }

        // 响应数据
        public string ResponseMessage { get; set; }
        public int State { get; set; }
        public int Result { get; set; }


        // 构造函数
        public TestStateMachineContext()
        {
            Reset();
        }

        // 重置上下文数据
        public void Reset()
        {
            TotalCycleCount = 0;
            SuccessCount = 0;
            StateStartTime = DateTime.Now;
            ResponseMessage = null;
            State = 0;
            Result = 0;
        }

        // 更新状态开始时间
        public void UpdateStateStartTime()
        {
            StateStartTime = DateTime.Now;
        }

        // 获取状态已运行时间（秒）
        public double GetElapsedSeconds()
        {
            return (DateTime.Now - StateStartTime).TotalSeconds;
        }
    }
}
