using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 正反转切换间隔状态
    public class RotationIntervalState : StateBase
    {
        // 进入状态时执行
        public override void Enter(TestStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            _logger.Info($"正反转切换间隔 {GetParameters(stateMachine).RotationInterval} 秒");
        }

        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            var context = GetContext(stateMachine);
            var parameters = GetParameters(stateMachine);

            // 等待间隔时间结束
            if (context.GetElapsedSeconds() >= parameters.RotationInterval)
            {
                return TestState.ReverseDelay;
            }

            // 继续等待
            return TestState.ReverseDelay;
        }
    }
}
