using Newtonsoft.Json;
using SasTools.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 等待正转结果状态
    public class ForwardWaitingState : StateBase
    {
        // 错误代码掩码常量
        private const int ERROR_MOTOR_STALL = 0x01;         // bit0: 锁付步骤失败，电机堵转
        private const int ERROR_USER_STOP = 0x02;           // bit1: 锁付步骤失败，用户停止
        private const int ERROR_ANGLE_LIMIT = 0x04;         // bit2: 锁付步骤失败，角度超限
        private const int ERROR_TIMEOUT = 0x08;             // bit3: 锁付步骤失败，超时
        private const int ERROR_MOTOR_ERROR = 0x10;         // bit4: 锁付步骤失败，电机错误
        private const int ERROR_INCLINATION = 0x20;         // bit5: 锁付步骤失败，倾角超限
        private const int ERROR_TORQUE_REACHED = 0x40;      // bit6: 锁付步骤失败，扭力到达

        // 执行状态逻辑
        public override Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            // 获取已经等待的时间
            var elapsedSeconds = GetContext(stateMachine).GetElapsedSeconds();

            // 检查是否超时
            if (elapsedSeconds > GetParameters(stateMachine).Timeout)
            {
                _logger.Warn($"正转等待超时: {elapsedSeconds}s");
                HandleTimeout(stateMachine);
                return Task.FromResult(TestState.RotationInterval);
            }

            // 继续等待消息处理，保持当前状态
            return Task.FromResult(TestState.ForwardWaiting);
        }

        // 处理消息
        public override void HandleMessage(TestStateMachine stateMachine, string message)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<dynamic>(message);

                // 检查是否为锁付结果回复
                if (response.reply == 203)
                {
                    int state = (int)response.state;
                    int result = (int)response.result;

                    // 更新上下文
                    var context = GetContext(stateMachine);
                    context.State = state;
                    context.Result = result;

                    // 如果锁付已停止，进行结果判断
                    if (state == 0)
                    {
                        stateMachine.StopTimeoutTimer();
                        bool isSuccess = result == 0;
                        string resultMessage = isSuccess ?
                            "正转操作成功" :
                            GenerateErrorMessage(result);

                        if (isSuccess)
                        {
                            stateMachine.IncrementSuccessCount();
                        }

                        _logger.Info($"正转操作完成: {(isSuccess ? "成功" : "失败")}");
                        stateMachine.NotifyTestResult(isSuccess, resultMessage, result);
                        MoveToNextState(stateMachine);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理消息失败: {ex.Message}", ex);
                stateMachine.NotifyError($"处理消息失败: {ex.Message}", -1);
                MoveToNextState(stateMachine);
            }
        }

        // 处理超时
        public override void HandleTimeout(TestStateMachine stateMachine)
        {
            try
            {
                // 发送停止指令
                ExecuteCommandAsync(stateMachine, CommandType.Stop).Wait(2000);
                stateMachine.NotifyTestResult(false, "正转操作超时", -1);
            }
            catch (Exception ex)
            {
                _logger.Error($"处理超时异常: {ex.Message}", ex);
            }
            finally
            {
                MoveToNextState(stateMachine);
            }
        }

        // 退出状态时执行
        public override void Exit(TestStateMachine stateMachine)
        {
            base.Exit(stateMachine);
            stateMachine.StopTimeoutTimer();
            stateMachine.IncrementCycleCount();
        }

        // 生成错误消息
        private string GenerateErrorMessage(int errorCode)
        {
            var msg = new System.Text.StringBuilder("正转操作失败: ");

            if ((errorCode & ERROR_MOTOR_STALL) != 0) msg.Append("电机堵转; ");
            if ((errorCode & ERROR_USER_STOP) != 0) msg.Append("用户停止; ");
            if ((errorCode & ERROR_ANGLE_LIMIT) != 0) msg.Append("角度超限; ");
            if ((errorCode & ERROR_TIMEOUT) != 0) msg.Append("操作超时; ");
            if ((errorCode & ERROR_MOTOR_ERROR) != 0) msg.Append("电机错误; ");
            if ((errorCode & ERROR_INCLINATION) != 0) msg.Append("倾角超限; ");
            if ((errorCode & ERROR_TORQUE_REACHED) != 0) msg.Append("扭力到达; ");

            // 如果没有匹配到具体错误
            if (msg.Length == 8)
            {
                msg.Append($"未知错误(代码:{errorCode})");
            }

            return msg.ToString();
        }

        // 移动到下一个状态
        private void MoveToNextState(TestStateMachine stateMachine)
        {
            stateMachine.GetContext().UpdateStateStartTime();
            stateMachine.GetType()
                        .GetProperty("CurrentState")
                        ?.SetValue(stateMachine, TestState.RotationInterval);
        }
    }
}



