using SasTools.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 初始化状态
    public class InitializingState : StateBase
    {
        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            try
            {
                // 发送订阅指令
                await ExecuteCommandAsync(stateMachine, CommandType.Subscribe);

                // 初始化完成，进入正转延时状态
                return TestState.ForwardDelay;
            }
            catch (Exception ex)
            {
                stateMachine.NotifyError($"初始化失败: {ex.Message}", -1);
                return TestState.Error;
            }
        }
    }
}

