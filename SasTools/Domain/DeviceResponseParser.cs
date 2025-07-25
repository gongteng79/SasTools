using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SasTools.Domain
{
    //设备响应解析器 - 统一处理设备JSON响应的解析逻辑
    public class DeviceResponseParser
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(DeviceResponseParser)); 

        public DeviceResponseParser(ILog logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //解析设备JSON响应数据
        public DeviceParseResult ParseResponse(string jsonMsg)
        {
            if (string.IsNullOrEmpty(jsonMsg))
            {
                _logger.Warn("设备返回空数据");
                return new DeviceParseResult
                {
                    Success = false,
                    State = -1,
                    Result = -1,
                    ErrorMessage = "设备返回空数据"
                };
            }

            try
            {
                _logger.Debug($"🔍 开始解析JSON: {jsonMsg}");

                var response = JsonConvert.DeserializeObject<dynamic>(jsonMsg);

                _logger.Debug($"🔍 JSON反序列化结果: {response}");

                if (response != null && response.reply == 203 && response.state != null)
                {
                    int state = (int)response.state;
                    int result = 0;

                    if (response.result != null)
                    {
                        result = (int)response.result;
                        _logger.Debug($"🔍 使用JSON中的result字段: {result}");
                    }
                    else
                    {
                        _logger.Debug($"🔍 JSON中无result字段，使用默认值: {result}");
                    }

                    _logger.Debug($"🔍 解析成功: state={state}, result={result}");

                    return new DeviceParseResult
                    {
                        Success = true,
                        State = state,
                        Result = result,
                        ErrorMessage = null,
                        RawJson = jsonMsg
                    };
                }
                else
                {
                    _logger.Warn($"🔍 JSON响应格式错误 - reply: {response?.reply}, state: {response?.state}, result: {response?.result}");
                    return new DeviceParseResult
                    {
                        Success = false,
                        State = -1,
                        Result = -1,
                        ErrorMessage = "JSON响应格式错误",
                        RawJson = jsonMsg
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"🔍 JSON解析异常: {ex.Message}, 原始数据: {jsonMsg}");
                return new DeviceParseResult
                {
                    Success = false,
                    State = -1,
                    Result = -1,
                    ErrorMessage = $"JSON解析失败: {ex.Message}",
                    RawJson = jsonMsg
                };
            }
        }

        //判断设备状态是否正常（无错误）
        public bool IsDeviceStateNormal(DeviceParseResult parseResult)
        {
            return parseResult.Success && parseResult.State == 0 && parseResult.Result == 0;
        }

        //判断操作是否成功（JSON协议特殊处理）
        public bool IsOperationSuccessful(DeviceParseResult parseResult)
        {
            if (!parseResult.Success)
            {
                _logger.Debug("解析失败，判定为操作失败");
                return false;
            }

            if (parseResult.State == 0)
            {
                bool isSuccessful = parseResult.Result == 0;

                if (isSuccessful)
                {
                    _logger.Debug($"✅ 锁付操作成功: state={parseResult.State}, result={parseResult.Result}");
                }
                else
                {
                    string errorDesc = GetLockErrorDescription(parseResult.Result);
                    _logger.Warn($"❌ 锁付操作失败: state={parseResult.State}, result={parseResult.Result}, 错误: {errorDesc}");
                }

                return isSuccessful;
            }
            else
            {
                _logger.Debug($"⏳ 设备工作中: state={parseResult.State}, 继续等待完成");
                return false;
            }
        }

        //检查设备是否正在执行操作
        public bool IsDeviceWorking(DeviceParseResult parseResult)
        {
            return parseResult.Success && parseResult.State != 0;
        }

        //检查设备是否已完成操作
        public bool IsOperationCompleted(DeviceParseResult parseResult)
        {
            return parseResult.Success && parseResult.State == 0;
        }

        //判断已完成的操作是否成功
        public bool IsCompletedOperationSuccessful(DeviceParseResult parseResult)
        {
            if (!IsOperationCompleted(parseResult))
            {
                throw new InvalidOperationException("操作尚未完成，无法判定成败");
            }

            return parseResult.Result == 0;
        }

        //获取锁付错误描述
        public string GetLockErrorDescription(int result)
        {
            if (result == 0) return "锁付成功";

            var errors = new List<string>();

            // 基础错误 (bit0-bit6)
            if ((result & (1 << 0)) != 0) errors.Add("电机堵转");
            if ((result & (1 << 1)) != 0) errors.Add("用户停止");
            if ((result & (1 << 2)) != 0) errors.Add("角度超限");
            if ((result & (1 << 3)) != 0) errors.Add("超时");
            if ((result & (1 << 4)) != 0) errors.Add("电机错误");
            if ((result & (1 << 5)) != 0) errors.Add("倾角超限");
            if ((result & (1 << 6)) != 0) errors.Add("扭力到达");

            // 参数超限错误 (bit17-bit29)
            if ((result & (1 << 17)) != 0) errors.Add("速度低于下限");
            if ((result & (1 << 18)) != 0) errors.Add("速度超过上限");
            if ((result & (1 << 19)) != 0) errors.Add("时间低于下限");
            if ((result & (1 << 20)) != 0) errors.Add("时间超过上限");
            if ((result & (1 << 21)) != 0) errors.Add("扭力低于下限");
            if ((result & (1 << 22)) != 0) errors.Add("扭力超过上限");
            if ((result & (1 << 23)) != 0) errors.Add("角度低于下限");
            if ((result & (1 << 24)) != 0) errors.Add("角度超过上限");
            if ((result & (1 << 25)) != 0) errors.Add("夹紧扭力低于下限");
            if ((result & (1 << 26)) != 0) errors.Add("夹紧扭力超过上限");
            if ((result & (1 << 27)) != 0) errors.Add("夹紧角度低于下限");
            if ((result & (1 << 28)) != 0) errors.Add("夹紧角度超过上限");
            if ((result & (1 << 29)) != 0) errors.Add("空转");

            return string.Join(", ", errors);
        }
    }

    //设备解析结果
    public class DeviceParseResult
    {
        //解析是否成功
        public bool Success { get; set; }

        //设备状态
        public int State { get; set; }

        //设备结果
        public int Result { get; set; }

        //错误消息
        public string ErrorMessage { get; set; }

        //原始JSON数据
        public string RawJson { get; set; }
    }
}
