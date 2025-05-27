using SasTools.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 正转操作状态
    public class ForwardState : StateBase
    {
        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            try
            {
                // 发送正转指令
                await ExecuteCommandAsync(stateMachine, CommandType.Forward);

                // 启动超时计时器
                int timeoutMs = GetParameters(stateMachine).Timeout * 1000;
                stateMachine.StartTimeoutTimer(timeoutMs);

                // 进入等待正转结果状态
                return TestState.ForwardWaiting;
            }
            catch (Exception ex)
            {
                stateMachine.NotifyError($"启动正转操作失败: {ex.Message}", -1);
                return TestState.Error;
            }
        }
    }
}

