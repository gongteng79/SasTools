using SasTools.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 反转操作状态
    public class ReverseState : StateBase
    {
        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            try
            {
                // 发送反转指令
                await ExecuteCommandAsync(stateMachine, CommandType.Reverse);

                // 启动超时计时器
                int timeoutMs = GetParameters(stateMachine).Timeout * 1000;
                stateMachine.StartTimeoutTimer(timeoutMs);

                // 进入等待反转结果状态
                return TestState.ReverseWaiting;
            }
            catch (Exception ex)
            {
                stateMachine.NotifyError($"启动反转操作失败: {ex.Message}", -1);
                return TestState.Error;
            }
        }
    }
}

