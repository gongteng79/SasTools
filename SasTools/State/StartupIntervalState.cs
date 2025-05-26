using SasTools.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 循环启动间隔状态
    public class StartupIntervalState : StateBase
    {
        // 进入状态时执行
        public override void Enter(TestStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            // 增加循环计数
            stateMachine.IncrementCycleCount();

            _logger.Info($"循环启动间隔 {GetParameters(stateMachine).StartupInterval} 秒，当前循环次数: {GetContext(stateMachine).TotalCycleCount}");
        }

        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            var context = GetContext(stateMachine);
            var parameters = GetParameters(stateMachine);

            // 等待间隔时间结束
            if (context.GetElapsedSeconds() >= parameters.StartupInterval)
            {
                return TestState.ForwardDelay;
            }

            // 继续等待
            return TestState.StartupInterval;
        }
    }
}
