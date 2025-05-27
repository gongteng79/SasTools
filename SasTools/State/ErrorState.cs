using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 错误状态
    public class ErrorState : StateBase
    {
        // 执行状态逻辑
        public override Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            // 保持错误状态，直到外部重置
            return Task.FromResult(TestState.Error);
        }
    }
}

