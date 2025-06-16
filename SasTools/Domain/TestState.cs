using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    // 测试状态机状态枚举
    public enum TestState
    {
        // 空闲状态
        Idle,

        // 初始化状态(101订阅)
        Initializing,

        // 正转延时
        ForwardDelay,

        // 正转操作(锁付ok后才会获取203)
        Forward,

        //传送锁付数据(获取203指令)
        InputScrewData,

        // 等待正转结果
        ForwardWaiting,

        // 反转延时
        ReverseDelay,

        // 反转操作(反转用时间来控制，可以直接发115指令中Time设置)
        Reverse,

        // 等待反转结果 （反转时间结束则为反转结束）
        ReverseWaiting,

        // 停止中
        Stopping,

        // 错误状态
        Error
    }

    // 测试结果类
    public class TestResult
    {
        // 是否成功
        public bool IsSuccess { get; set; }

        // 消息
        public string Message { get; set; }

        // 错误代码
        public int ErrorCode { get; set; }
    }

    // 测试计数器事件参数
    public class TestCounterEventArgs : EventArgs
    {
        // 总循环次数
        public int TotalCycles { get; set; }

        // 成功次数
        public int SuccessCount { get; set; }
    }
}

