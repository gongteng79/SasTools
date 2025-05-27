using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 反转延时状态
    public class ReverseDelayState : StateBase
    {
        // 进入状态时执行
        public override void Enter(TestStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            _logger.Info($"反转延时 {GetParameters(stateMachine).ReverseDelay} 秒");
        }

        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            var context = GetContext(stateMachine);
            var parameters = GetParameters(stateMachine);

            // 等待延时时间结束
            if (context.GetElapsedSeconds() >= parameters.ReverseDelay)
            {
                return TestState.Reverse;
            }

            // 继续等待
            return TestState.ReverseDelay;
        }
    }
}

