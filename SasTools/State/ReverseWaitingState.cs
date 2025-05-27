using Newtonsoft.Json;
using SasTools.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SasTools.States
{
    // 等待反转结果状态
    public class ReverseWaitingState : StateBase
    {
        // 执行状态逻辑
        public override async Task<TestState> ProcessAsync(TestStateMachine stateMachine, CancellationToken cancellationToken)
        {
            // 获取已经等待的时间
            var elapsedSeconds = GetContext(stateMachine).GetElapsedSeconds();

            // 每隔1秒发送一次状态查询
            if (Math.Floor(elapsedSeconds) % 1 == 0)
            {
                try
                {
                    // 发送状态查询指令
                    string response = await ExecuteCommandAsync(stateMachine, CommandType.StatusQuery);
                    CheckReverseStatus(stateMachine, response);
                }
                catch (Exception ex)
                {
                    _logger.Error($"状态查询失败: {ex.Message}", ex);
                }
            }

            // 检查是否超时
            if (elapsedSeconds > GetParameters(stateMachine).Timeout)
            {
                _logger.Warn($"反转等待超时: {elapsedSeconds}s");
                HandleTimeout(stateMachine);
                return TestState.StartupInterval;
            }

            // 继续等待，保持当前状态
            return TestState.ReverseWaiting;
        }

        // 检查反转状态
        private void CheckReverseStatus(TestStateMachine stateMachine, string response)
        {
            try
            {
                var responseObj = JsonConvert.DeserializeObject<dynamic>(response);

                // 检查是否为状态查询响应
                if (responseObj.reply == 219)
                {
                    int loosen = (int)responseObj.loosen;        // 拆螺丝状态
                    int offline = (int)responseObj.offline;      // 离线状态
                    int torqueTest = (int)responseObj.torque_test; // 扭力测试状态

                    // 判断反转操作是否完成
                    if (loosen == 0)  // 反转操作已完成
                    {
                        stateMachine.StopTimeoutTimer();

                        // 检查是否有错误
                        bool hasError = offline == 1 || torqueTest == 1;
                        bool isSuccess = !hasError;

                        string resultMessage = isSuccess ?
                            "反转操作成功" :
                            GenerateErrorMessage(offline == 1, torqueTest == 1);

                        if (isSuccess)
                        {
                            stateMachine.IncrementSuccessCount();
                        }

                        _logger.Info($"反转操作完成: {(isSuccess ? "成功" : "失败")}");
                        stateMachine.NotifyTestResult(isSuccess, resultMessage, isSuccess ? 0 : -1);
                        MoveToNextState(stateMachine);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理状态查询响应失败: {ex.Message}", ex);
            }
        }

        // 处理超时
        public override void HandleTimeout(TestStateMachine stateMachine)
        {
            try
            {
                // 发送停止指令
                ExecuteCommandAsync(stateMachine, CommandType.Stop).Wait(2000);
                stateMachine.NotifyTestResult(false, "反转操作超时", -1);
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

        // 处理消息
        public override void HandleMessage(TestStateMachine stateMachine, string message)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<dynamic>(message);

                // 如果是状态查询响应
                if (response.reply == 219)
                {
                    CheckReverseStatus(stateMachine, message);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理消息失败: {ex.Message}", ex);
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
        private string GenerateErrorMessage(bool isOffline, bool hasTorqueError)
        {
            var msg = new System.Text.StringBuilder("反转操作失败: ");

            if (isOffline) msg.Append("设备离线; ");
            if (hasTorqueError) msg.Append("扭矩异常; ");

            return msg.ToString();
        }

        private void MoveToNextState(TestStateMachine stateMachine)
        {
            stateMachine.GetContext().UpdateStateStartTime();
            stateMachine.GetType()
                        .GetProperty("CurrentState")
                        ?.SetValue(stateMachine, TestState.StartupInterval);
        }
    }
}




