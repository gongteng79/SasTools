using log4net;
using Newtonsoft.Json;
using SasTools.Interface;
using SasTools.Models.SasModule;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SasTools.Domain.TestStateMachine;

namespace SasTools.Domain
{
    //设备状态管理器 - 统一管理设备状态检查、清理和验证
    public class DeviceStateManager
    {
        private readonly ILog _logger;
        private readonly DeviceResponseParser _responseParser;

        public DeviceStateManager(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _responseParser = new DeviceResponseParser(_logger);
        }

        //重置设备状态 - 统一的设备重置方法
        public async Task ResetDeviceStateAsync(IDevice device, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info("开始统一设备状态重置");

                // 多次停止命令确保设备完全停止
                device.ExecuteCommand(SasCommandType.Stop);
                await Task.Delay(Constants.DEVICE_STOP_WAIT_TIME, cancellationToken);

                device.ExecuteCommand(SasCommandType.Stop);
                await Task.Delay(200, cancellationToken);

                // 多次清理设备错误信息
                device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME, cancellationToken);

                device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME, cancellationToken);

                // 重新订阅
                device.ExecuteCommand(SasCommandType.Subscribe);
                await Task.Delay(500, cancellationToken);

                // 验证设备状态
                await VerifyDeviceStateAsync(device, cancellationToken);

                _logger.Info("统一设备状态重置完成");
            }
            catch (OperationCanceledException)
            {
                _logger.Info("设备状态重置被取消");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"统一设备状态重置失败: {ex.Message}", ex);
                throw;
            }
        }

        //检查并清理设备状态 - 统一的设备状态检查方法
        public async Task CheckAndCleanDeviceStateAsync(IDevice device, CancellationToken cancellationToken, bool includeSubscribe = false)
        {
            try
            {
                _logger.Debug("检查并清理设备状态");

                if (includeSubscribe)
                {
                    device.ExecuteCommand(SasCommandType.Subscribe);
                    await Task.Delay(300, cancellationToken);
                }

                //实际的状态检查逻辑
                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);
                if (!string.IsNullOrEmpty(jsonMsg))
                {
                    var parseResult = _responseParser.ParseResponse(jsonMsg);

                    _logger.Debug($"状态检查 - JSON: {jsonMsg}");
                    _logger.Debug($"状态检查 - 解析结果: Success={parseResult.Success}, State={parseResult.State}, Result={parseResult.Result}");

                    if (parseResult.Success)
                    {
                        // 检查是否有错误状态需要清理
                        if (!_responseParser.IsDeviceStateNormal(parseResult))
                        {
                            string errorDesc = _responseParser.GetLockErrorDescription(parseResult.Result);
                            _logger.Info($"检测到设备错误状态，执行清理: state={parseResult.State}, result={parseResult.Result}, 错误: {errorDesc}");

                            // 清理错误状态
                            device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                            await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME, cancellationToken);

                            _logger.Debug("设备状态清理完成");
                        }
                        else
                        {
                            _logger.Debug("设备状态正常，无需清理");
                        }
                    }
                    else
                    {
                        _logger.Warn($"设备状态检查失败: {parseResult.ErrorMessage}");
                    }
                }
                else
                {
                    _logger.Warn("设备状态检查返回空数据");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"检查设备状态失败: {ex.Message}");
            }
        }

        //验证设备状态是否已清理干净
        private async Task VerifyDeviceStateAsync(IDevice device, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info("验证设备状态是否已清理干净");

                await Task.Delay(300, cancellationToken);

                string jsonMsg = device.ExecuteCommand(SasCommandType.InputScrewData);
                if (!string.IsNullOrEmpty(jsonMsg))
                {
                    var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);
                    if (response?.reply == 203 && response.state != null && response.result != null)
                    {
                        int state = (int)response.state;
                        int result = (int)response.result;

                        if (state != 0 || result != 0)
                        {
                            _logger.Warn($"设备状态未完全清理: state={state}, result={result}");

                            device.ExecuteCommand(SasCommandType.ClearTightenInfo);
                            await Task.Delay(Constants.DEVICE_CLEAR_WAIT_TIME, cancellationToken);

                            _logger.Info("已执行额外的设备状态清理");
                        }
                        else
                        {
                            _logger.Info("设备状态验证通过");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"验证设备状态失败: {ex.Message}");
            }
        }
    }
}