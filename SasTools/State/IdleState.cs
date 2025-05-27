using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 空闲状态
    public class IdleState : StateBase
    {
        // 执行状态逻辑
        public override Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            // 空闲状态不执行任何操作，保持当前状态
            return Task.FromResult(TestState.Idle);
        }
    }
}

