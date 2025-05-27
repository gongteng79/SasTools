using log4net;
using SasTools.Domain;
using SasTools.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 状态基类 - 提供通用功能
    public abstract class StateBase : IState
    {
        protected readonly ILog _logger = LogManager.GetLogger(typeof(StateBase));

        // 进入状态时执行
        public virtual void Enter(TestStateMachine stateMachine)
        {
            // 更新状态开始时间
            stateMachine.GetContext().UpdateStateStartTime();
            _logger.Debug($"进入状态: {GetType().Name}");
        }

        // 执行状态逻辑
        public abstract Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken);

        // 处理消息
        public virtual void HandleMessage(TestStateMachine stateMachine, string message)
        {
            
        }

        // 处理超时
        public virtual void HandleTimeout(TestStateMachine stateMachine)
        {
            // 默认超时处理
            stateMachine.NotifyError($"状态 {GetType().Name} 操作超时", -1);
        }

        // 退出状态时执行
        public virtual void Exit(TestStateMachine stateMachine)
        {
            _logger.Debug($"退出状态: {GetType().Name}");
        }

        // 获取参数
        protected TestParameters GetParameters(TestStateMachine stateMachine)
        {
            return stateMachine.GetContext().Parameters;
        }

        // 获取上下文
        protected TestStateMachineContext GetContext(TestStateMachine stateMachine)
        {
            return stateMachine.GetContext();
        }

        // 发送命令
        protected async Task<string> ExecuteCommandAsync(TestStateMachine stateMachine, CommandType commandType, object parameters = null)
        {
            try
            {
                return await stateMachine.GetCommandService().ExecuteCommandAsync(commandType, parameters);
            }
            catch (Exception ex)
            {
                _logger.Error($"执行命令 {commandType} 失败: {ex.Message}", ex);
                throw;
            }
        }
    }
}

