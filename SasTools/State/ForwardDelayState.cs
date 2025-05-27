using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 正转延时状态
    public class ForwardDelayState : StateBase
    {
        // 进入状态时执行
        public override void Enter(TestStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            _logger.Info($"正转延时 {GetParameters(stateMachine).ForwardDelay} 秒");
        }

        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            var context = GetContext(stateMachine);
            var parameters = GetParameters(stateMachine);

            // 等待延时时间结束
            if (context.GetElapsedSeconds() >= parameters.ForwardDelay)
            {
                return TestState.Forward;
            }

            // 继续等待
            return TestState.ForwardDelay;
        }
    }
}

