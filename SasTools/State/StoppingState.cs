using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 停止中状态
    public class StoppingState : StateBase
    {

        // 执行状态逻辑
        public override Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            // 停止过程完成后转为空闲状态
            return Task.FromResult(TestState.Idle);
        }
    }
}

