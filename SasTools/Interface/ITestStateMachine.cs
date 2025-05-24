using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasTools.Domain;

namespace SasTools.Interface
{
    //测试状态机状态
    public interface ITestStateMachine
    {
        //当前状态
        TestState CurrentState { get; }

        //状态变化事件
        event EventHandler<TestState> StateChanged;

        //测试结果事件
        event EventHandler<TestResult> TestResultReceived;

        //开始测试
        Task<bool> StartTestAsync();

        //停止测试
        Task<bool> StopTestAsync();

        //复位 - 清除状态和计数器
        Task<bool> ResetAsync();

        //处理接收到的消息
        void HandleMessage(string message);
    }
}
