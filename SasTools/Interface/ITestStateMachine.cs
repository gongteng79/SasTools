using SasTools.Domain;
using System;
using System.Threading.Tasks;

namespace SasTools.Interface
{

    // 测试状态机接口
    public interface ITestStateMachine
    {
        // 当前状态
        TestState CurrentState { get; }

        // 状态变化事件
        event EventHandler<TestState> StateChanged;

        // 测试结果事件
        event EventHandler<TestResult> TestResultReceived;

        // 测试计数变化事件
        event EventHandler<TestCounterEventArgs> CounterChanged;

        // 启动测试
        Task<bool> StartTestAsync();

        // 停止测试
        Task<bool> StopTestAsync();

        // 复位 - 清除状态和计数器
        Task<bool> ResetAsync();

        // 处理接收到的消息
        void HandleMessage(string message);

        // 重置计数器
        void ResetCounters();
    }
}

