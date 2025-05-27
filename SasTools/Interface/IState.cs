using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    // 状态接口 - 所有状态类必须实现此接口
    public interface IState
    {
        // 进入状态时执行
        void Enter(TestStateMachine stateMachine);

        // 执行状态逻辑，返回下一个状态
        Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken);

        // 处理接收到的消息
        void HandleMessage(TestStateMachine stateMachine, string message);

        // 处理超时
        void HandleTimeout(TestStateMachine stateMachine);

        // 退出状态时执行
        void Exit(TestStateMachine stateMachine);
    }
}


