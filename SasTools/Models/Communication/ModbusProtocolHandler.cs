using HslCommunication.ModBus;
using HslCommunication.Core;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using SasTools.Domain;
using SasTools.Interface;
using SasTools.Models.Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WpFramework.LogFactory;

namespace SasTools.Models.Communication
{
    public class ModbusProtocolHandler : IProtocolHandler
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ModbusProtocolHandler));
        private ModbusTcpNet _modbusClient;
        private ModbusProtocolConfig _config;
        private bool _disposed = false;
        private bool _isDeviceInitialized = false;
        private bool _isConnected = false;
        private int _addressOffset = 0;
        private IByteTransform _byteTransform; // 添加字节序转换器

        public ProtocolType ProtocolType => ProtocolType.ModbusTcp;
        public bool IsConnected => _isConnected;

        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<string> MessageReceived;

        public ModbusProtocolHandler()
        {
            //16位使用AB，32位使用CDAB
            //大端序
            _byteTransform = new RegularByteTransform();
            _byteTransform.DataFormat = DataFormat.CDAB;
        }

        private async Task<DeviceResponse> InitializeDeviceAsync()
        {
            try
            {
                _logger.Info($"开始Modbus设备初始化序列 - 设备: {_config.Host}:{_config.Port}");

                // 如果已经初始化过，直接返回成功
                if (_isDeviceInitialized)
                {
                    _logger.Debug("设备已初始化，跳过初始化序列");
                    return new DeviceResponse { Success = true, Message = "设备已初始化" };
                }
                var protocolVerifyResult = await VerifyProtocolAlignment();
                if (!protocolVerifyResult.Success)
                {
                    return protocolVerifyResult;
                }

                // 步骤1: 设备状态检查和清理
                _logger.Debug("步骤1: 执行设备状态清理");
                var clearDataResult = await WriteRegisterWithTimeout(ModbusRegisterMap.CLEAR_LOCK_DATA, (short)1);
                if (!clearDataResult.Success)
                {
                    _logger.Error($"设备状态清理失败: {clearDataResult.Message}");
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"初始化失败: 设备状态清理失败 - {clearDataResult.Message}"
                    };
                }
                _logger.Debug("设备状态清理完成");
                await Task.Delay(500);

                // 步骤2: 设置基础配置参数
                _logger.Debug("步骤2: 设置基础配置参数");

                // 设置当前产品编号 (地址120, CURRENT_PRODUCT_NUMBER)
                var productNumberResult = await WriteRegisterWithTimeout(ModbusRegisterMap.CURRENT_PRODUCT_NUMBER, (short)0);
                if (!productNumberResult.Success)
                {
                    _logger.Error($"设置产品编号失败: {productNumberResult.Message}");
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"初始化失败: 设置产品编号失败 - {productNumberResult.Message}"
                    };
                }
                _logger.Debug("产品编号已设置为0");
                await Task.Delay(200);

                // 设置当前螺丝规格编号 (地址122, CURRENT_SCREW_SPEC)
                var screwSpecResult = await WriteRegisterWithTimeout(ModbusRegisterMap.CURRENT_SCREW_SPEC, (short)0);
                if (!screwSpecResult.Success)
                {
                    _logger.Error($"设置螺丝规格编号失败: {screwSpecResult.Message}");
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"初始化失败: 设置螺丝规格编号失败 - {screwSpecResult.Message}"
                    };
                }
                _logger.Debug("螺丝规格编号已设置为0");
                await Task.Delay(200);

                // 步骤3: 启用电批通电控制 (地址180, POWER_ENABLE)
                _logger.Debug("步骤3: 启用电批通电控制");
                var powerEnableResult = await WriteRegisterWithTimeout(ModbusRegisterMap.POWER_ENABLE, (short)1);
                if (!powerEnableResult.Success)
                {
                    _logger.Error($"启用电批通电控制失败: {powerEnableResult.Message}");
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"初始化失败: 启用电批通电控制失败 - {powerEnableResult.Message}"
                    };
                }
                _logger.Debug("电批通电控制已启用");
                await Task.Delay(1000); // 增加等待时间确保生效

                // 步骤4: 最终清理确保数据清洁 (地址107, CLEAR_LOCK_DATA)
                _logger.Debug("步骤4: 执行最终清理");
                var finalClearResult = await WriteRegisterWithTimeout(ModbusRegisterMap.CLEAR_LOCK_DATA, (short)1);
                if (!finalClearResult.Success)
                {
                    _logger.Error($"最终清理失败: {finalClearResult.Message}");
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"初始化失败: 最终清理失败 - {finalClearResult.Message}"
                    };
                }
                _logger.Debug("最终清理完成");
                await Task.Delay(200);

                // 步骤5: 验证初始化结果 - 读取设备状态确认初始化成功
                var verifyResult = await VerifyInitializationAsync();
                if (!verifyResult.Success)
                {
                    _logger.Error($"设备初始化验证失败: {verifyResult.Message}");
                    return verifyResult;
                }

                _isDeviceInitialized = true;
                _logger.Info("Modbus设备初始化序列完成");

                return new DeviceResponse
                {
                    Success = true,
                    Message = "Modbus设备初始化成功"
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Modbus设备初始化异常: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = $"初始化异常: {ex.Message}"
                };
            }
        }

        private async Task<DeviceResponse> VerifyProtocolAlignment()
        {
            try
            {
                _logger.Info("开始协议地址偏移和字节序验证");

                // 1. 检测地址偏移
                var offsetResult = _modbusClient.Read(ModbusRegisterMap.PROTOCOL_OFFSET, 1);
                if (!offsetResult.IsSuccess)
                {
                    _logger.Warn($"无法读取协议偏移地址: {offsetResult.Message}，使用默认偏移0");
                    _addressOffset = 0;
                    return new DeviceResponse { Success = true, Message = "跳过协议验证" };
                }

                int offsetValue = _byteTransform.TransUInt16(offsetResult.Content, 0);
                _logger.Info($"协议地址偏移检测值: {offsetValue} (0x{offsetValue:X4})");

                // 根据检测值设置地址偏移
                switch (offsetValue)
                {
                    case 0:
                        _addressOffset = 0;
                        _logger.Info("地址正确，无需偏移");
                        break;
                    case 4369: // 0x1111
                        _addressOffset = 1;
                        _logger.Info("检测到地址+1偏移，已应用修正");
                        break;
                    case 8738: // 0x2222
                        _addressOffset = 2;
                        _logger.Info("检测到地址+2偏移，已应用修正");
                        break;
                    case 34952: // 0x8888
                        _addressOffset = -1;
                        _logger.Info("检测到地址-1偏移，已应用修正");
                        break;
                    case 17476: // 0x4444
                        _addressOffset = -2;
                        _logger.Info("检测到地址-2偏移，已应用修正");
                        break;
                    default:
                        _addressOffset = 0;
                        _logger.Warn($"未知偏移值: {offsetValue}，使用默认偏移0");
                        break;
                }

                // 2. 验证字节序转换
                await VerifyByteOrderAsync();

                _logger.Info($"协议地址偏移验证完成，偏移量: {_addressOffset}");
                return new DeviceResponse { Success = true, Message = "协议验证成功" };
            }
            catch (Exception ex)
            {
                _logger.Error($"协议验证异常: {ex.Message}", ex);
                _addressOffset = 0;
                return new DeviceResponse
                {
                    Success = false,
                    Message = $"协议验证异常: {ex.Message}"
                };
            }
        }

        // 新增字节序验证方法
        private async Task VerifyByteOrderAsync()
        {
            try
            {
                _logger.Info("开始字节序验证");

                // 验证16位整数 (期望值: 258/0x0102)
                var test16Result = _modbusClient.Read(ModbusRegisterMap.TEST_16BIT, 1);
                if (test16Result.IsSuccess)
                {
                    int value16_default = _byteTransform.TransUInt16(test16Result.Content, 0);
                    _logger.Info($"16位整数测试值(默认): {value16_default} (0x{value16_default:X4}), 期望: 258 (0x0102)");

                    if (value16_default != 258)
                    {
                        if (test16Result.Content.Length >= 2)
                        {
                            int value16_swapped = (test16Result.Content[1] << 8) | test16Result.Content[0];
                            _logger.Info($"16位整数测试值(字节交换): {value16_swapped} (0x{value16_swapped:X4})");
                        }

                        if (value16_default == 772) // 0x0304，需要字节交换
                        {
                            _byteTransform.DataFormat = DataFormat.BADC;
                            _logger.Info("调整为BADC字节序");
                        }
                    }
                    else
                    {
                        _logger.Info("16位整数测试通过");
                    }
                }

                // 验证32位整数 - 使用直接地址测试
                _logger.Info("尝试直接读取32位测试地址");
                var directTest32Result = _modbusClient.Read(AdjustAddress("8"), 2);
                if (directTest32Result.IsSuccess)
                {
                    _logger.Info($"32位测试地址原始数据: {BitConverter.ToString(directTest32Result.Content)}");

                    // 尝试不同字节序
                    var formats = new[] { DataFormat.ABCD, DataFormat.BADC, DataFormat.CDAB, DataFormat.DCBA };
                    foreach (var format in formats)
                    {
                        var tempTransform = new RegularByteTransform { DataFormat = format };
                        int testValue = tempTransform.TransInt32(directTest32Result.Content, 0);
                        _logger.Info($"测试字节序 {format}: {testValue} (0x{testValue:X8})");

                        if (testValue == 16909060)
                        {
                            _byteTransform.DataFormat = format;
                            _logger.Info($"找到正确的32位字节序: {format}");
                            break;
                        }
                    }
                }

                // 验证32位浮点数 - 使用直接地址测试
                _logger.Info("尝试直接读取32位浮点测试地址");
                var directTestFloatResult = _modbusClient.Read(AdjustAddress("10"), 2);
                if (directTestFloatResult.IsSuccess)
                {
                    _logger.Info($"32位浮点测试地址原始数据: {BitConverter.ToString(directTestFloatResult.Content)}");

                    var formats = new[] { DataFormat.ABCD, DataFormat.BADC, DataFormat.CDAB, DataFormat.DCBA };
                    foreach (var format in formats)
                    {
                        var tempTransform = new RegularByteTransform { DataFormat = format };
                        float testValue = tempTransform.TransSingle(directTestFloatResult.Content, 0);
                        _logger.Info($"测试浮点字节序 {format}: {testValue}");

                        if (Math.Abs(testValue - 12.34f) < 0.01f)
                        {
                            _logger.Info($"找到正确的浮点字节序: {format}");
                            break;
                        }
                    }
                }

                _logger.Info($"字节序验证完成，最终使用字节序: {_byteTransform.DataFormat}");
            }
            catch (Exception ex)
            {
                _logger.Error($"字节序验证异常: {ex.Message}", ex);
            }
        }
        // 添加地址调整方法
        private string AdjustAddress(string address)
        {
            if (int.TryParse(address, out int addr))
            {
                int adjustedAddr = addr + _addressOffset;
                _logger.Debug($"地址调整: {address} -> {adjustedAddr} (偏移: {_addressOffset})");
                return adjustedAddr.ToString();
            }
            return address;
        }

        private async Task<DeviceResponse> VerifyInitializationAsync()
        {
            try
            {
                _logger.Debug("验证设备初始化状态");

                await Task.Delay(1000);

                // 读取电批通电控制状态
                var powerStatusResult = _modbusClient.Read(AdjustAddress(ModbusRegisterMap.POWER_ENABLE), 1);
                if (!powerStatusResult.IsSuccess)
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"验证失败: 无法读取电批通电状态 - {powerStatusResult.Message}"
                    };
                }

                _logger.Debug($"电批通电控制原始数据: {BitConverter.ToString(powerStatusResult.Content)}");
                int powerStatus = _byteTransform.TransUInt16(powerStatusResult.Content, 0);
                _logger.Debug($"电批通电控制状态: {powerStatus}");

                if (powerStatus != 1)
                {
                    _logger.Warn($"电批通电控制未启用，尝试重新设置");

                    // 重新设置电批通电控制
                    var retryResult = await WriteRegisterWithTimeout(ModbusRegisterMap.POWER_ENABLE, (short)1);
                    if (retryResult.Success)
                    {
                        await Task.Delay(1000);

                        // 重新验证
                        var retryStatusResult = _modbusClient.Read(AdjustAddress(ModbusRegisterMap.POWER_ENABLE), 1);
                        if (retryStatusResult.IsSuccess)
                        {
                            int retryStatus = _byteTransform.TransUInt16(retryStatusResult.Content, 0);
                            _logger.Debug($"重新设置后电批通电控制状态: {retryStatus}");

                            if (retryStatus == 1)
                            {
                                _logger.Info("重新设置电批通电控制成功");
                            }
                            else
                            {
                                return new DeviceResponse
                                {
                                    Success = false,
                                    Message = $"验证失败: 电批通电控制重新设置后仍未启用，当前值: {retryStatus}"
                                };
                            }
                        }
                    }
                }

                // 验证产品编号设置
                var productResult = _modbusClient.Read(AdjustAddress(ModbusRegisterMap.CURRENT_PRODUCT_NUMBER), 1);
                if (productResult.IsSuccess)
                {
                    int productNumber = _byteTransform.TransUInt16(productResult.Content, 0);
                    _logger.Debug($"当前产品编号: {productNumber}");
                }

                // 验证螺丝规格编号设置
                var screwSpecResult = _modbusClient.Read(AdjustAddress(ModbusRegisterMap.CURRENT_SCREW_SPEC), 1);
                if (screwSpecResult.IsSuccess)
                {
                    int screwSpec = _byteTransform.TransUInt16(screwSpecResult.Content, 0);
                    _logger.Debug($"当前螺丝规格编号: {screwSpec}");
                }

                _logger.Debug("设备初始化验证通过");
                return new DeviceResponse { Success = true, Message = "初始化验证成功" };
            }
            catch (Exception ex)
            {
                _logger.Error($"初始化验证异常: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = $"验证异常: {ex.Message}"
                };
            }
        }

        public async Task<bool> ConnectAsync(ProtocolConfig config)
        {
            if (!(config is ModbusProtocolConfig modbusConfig))
                throw new ArgumentException("Invalid config type for Modbus protocol");

            try
            {
                _config = modbusConfig;
                _modbusClient = new ModbusTcpNet(_config.Host, _config.Port, _config.SlaveId);

                var result = _modbusClient.ConnectServer();

                if (result.IsSuccess)
                {
                    _isConnected = true;
                    _logger.Info($"Modbus协议连接成功 - {_config.Host}:{_config.Port}");

                    // 不在连接时立即初始化，而是在第一次执行命令时初始化
                    _logger.Debug("连接成功，设备初始化将在首次命令执行时进行");
                }
                else
                {
                    _logger.Error($"Modbus协议连接失败: {result.Message}");
                    _isConnected = false;
                    _isDeviceInitialized = false;
                }

                ConnectionStatusChanged?.Invoke(this, _isConnected);
                return _isConnected;
            }
            catch (Exception ex)
            {
                _logger.Error($"Modbus协议连接失败: {ex.Message}", ex);
                _isConnected = false;
                _isDeviceInitialized = false;
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                if (_modbusClient != null)
                {
                    var result = _modbusClient.ConnectClose();
                    _logger.Info($"Modbus断开连接: {result.Message}");

                    // 重置连接和初始化状态
                    _isConnected = false;
                    _isDeviceInitialized = false;
                    _logger.Debug("设备连接和初始化状态已重置");

                    ConnectionStatusChanged?.Invoke(this, _isConnected);
                    return result.IsSuccess;
                }

                _isConnected = false;
                _isDeviceInitialized = false;
                ConnectionStatusChanged?.Invoke(this, _isConnected);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Modbus断开连接失败: {ex.Message}", ex);
                _isConnected = false;
                _isDeviceInitialized = false;
                ConnectionStatusChanged?.Invoke(this, _isConnected);
                return false;
            }
        }

        public async Task<DeviceResponse> ExecuteCommandAsync(SasCommandType commandType, CommandParameters parameters = null)
        {
            try
            {
                // 添加连接状态检查
                if (!_isConnected)
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = "设备未连接，无法执行命令"
                    };
                }

                // 初始化状态检查
                if (commandType != SasCommandType.Subscribe && !_isDeviceInitialized)
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = "设备未初始化，请先重新连接设备"
                    };
                }

                switch (commandType)
                {
                    case SasCommandType.Subscribe:
                        // Subscribe命令触发重新初始化
                        return await InitializeDeviceAsync();

                    case SasCommandType.Forward:
                        _logger.Info($"执行Forward命令 - 设备: {_config.Host}:{_config.Port}");
                        var forwardResult = await WriteRegisterWithTimeout(ModbusRegisterMap.START_LOCK, (short)1);
                        _logger.Info($"Forward命令执行结果: {forwardResult.Success}, 消息: {forwardResult.Message}");
                        return forwardResult;

                    case SasCommandType.Reverse:
                        _logger.Info($"执行Reverse命令 - 时间: {parameters?.Time}, 速度: {parameters?.Velocity}");

                        // 先设置参数
                        if (parameters?.Time > 0)
                        {
                            var timeResult = await WriteRegisterWithTimeout(ModbusRegisterMap.LOOSE_AUTO_STOP_TIME, (short)parameters.Time.Value);
                            if (!timeResult.Success)
                            {
                                return new DeviceResponse
                                {
                                    Success = false,
                                    Message = $"设置松螺丝时间失败: {timeResult.Message}"
                                };
                            }
                            _logger.Debug($"松螺丝时间设置成功: {parameters.Time.Value}ms");
                        }

                        if (parameters?.Velocity > 0)
                        {
                            var velocityResult = await WriteRegisterWithTimeout(ModbusRegisterMap.LOOSE_SPEED, (short)parameters.Velocity.Value);
                            if (!velocityResult.Success)
                            {
                                return new DeviceResponse
                                {
                                    Success = false,
                                    Message = $"设置松螺丝速度失败: {velocityResult.Message}"
                                };
                            }
                            _logger.Debug($"松螺丝速度设置成功: {parameters.Velocity.Value}");
                        }

                        // 启动松螺丝
                        var reverseResult = await WriteRegisterWithTimeout(ModbusRegisterMap.START_LOOSE, (short)1);
                        if (reverseResult.Success)
                        {
                            _logger.Info("反转命令发送成功");
                            return new DeviceResponse
                            {
                                Success = true,
                                Message = "反转命令执行成功"
                            };
                        }
                        else
                        {
                            _logger.Error($"反转命令发送失败: {reverseResult.Message}");
                            return reverseResult;
                        }

                    case SasCommandType.Stop:
                        _logger.Info("执行Stop命令");
                        var stopLockResult = await WriteRegisterWithTimeout(ModbusRegisterMap.STOP_LOCK, (short)1);
                        var stopLooseResult = await WriteRegisterWithTimeout(ModbusRegisterMap.STOP_LOOSE, (short)1);

                        return new DeviceResponse
                        {
                            Success = stopLockResult.Success && stopLooseResult.Success,
                            Message = $"停止锁付: {stopLockResult.Message}, 停止松螺丝: {stopLooseResult.Message}"
                        };

                    case SasCommandType.ClearTightenInfo:
                        _logger.Info("执行ClearTightenInfo命令");
                        var clearResult = await WriteRegisterWithTimeout(ModbusRegisterMap.CLEAR_LOCK_DATA, (short)1);
                        return clearResult;

                    case SasCommandType.InputScrewData:
                    case SasCommandType.StatusQuery:
                        return await ReadDataAsync();

                    default:
                        return new DeviceResponse
                        {
                            Success = false,
                            Message = $"Modbus协议不支持命令: {commandType}"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Modbus命令执行失败: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<DeviceResponse> ReadDataAsync()
        {
            try
            {
                // 添加连接状态检查
                if (!_isConnected)
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = "设备未连接，无法读取数据"
                    };
                }

                var (stateSuccess, stateData, stateMessage) = await ReadRegisterWithTimeout(ModbusRegisterMap.LOCK_STATE, 1);
                if (!stateSuccess)
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"读取状态失败: {stateMessage}"
                    };
                }

                var (resultSuccess, resultData, resultMessage) = await ReadRegisterWithTimeout(ModbusRegisterMap.LOCK_RESULT, 2);
                if (!resultSuccess)
                {
                    return new DeviceResponse
                    {
                        Success = false,
                        Message = $"读取结果失败: {resultMessage}"
                    };
                }

                _logger.Debug($"状态原始数据: {BitConverter.ToString(stateData)}");
                _logger.Debug($"结果原始数据: {BitConverter.ToString(resultData)}");

                int state = _byteTransform.TransUInt16(stateData, 0);
                int result = _byteTransform.TransInt32(resultData, 0);

                _logger.Debug($"读取设备数据成功 - 状态: {state}, 结果: {result}");

                return new DeviceResponse
                {
                    Success = true,
                    State = state,
                    Result = result,
                    Reply = 203,
                    Message = JsonConvert.SerializeObject(new
                    {
                        reply = 203,
                        state,
                        result
                    }),
                    Data = new Dictionary<string, object>
                    {
                        ["modbus_state"] = state,
                        ["modbus_result"] = result
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Modbus数据读取失败: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        //带超时控制的寄存器写入方法
        private async Task<DeviceResponse> WriteRegisterWithTimeout(string address, short value, int timeoutMs = 5000)
        {
            try
            {
                string adjustedAddress = AdjustAddress(address);

                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    var writeTask = Task.Run(() => _modbusClient.Write(adjustedAddress, value), cts.Token);
                    var result = await writeTask;

                    _logger.Debug($"寄存器写入完成 - 原地址: {address}, 调整后: {adjustedAddress}, 值: {value}, 结果: {result.IsSuccess}");

                    return new DeviceResponse
                    {
                        Success = result.IsSuccess,
                        Message = result.IsSuccess ? "写入成功" : $"写入失败: {result.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"寄存器写入异常 - 地址: {address}, 值: {value}, 异常: {ex.Message}", ex);
                return new DeviceResponse
                {
                    Success = false,
                    Message = $"寄存器写入异常: {ex.Message}"
                };
            }
        }

        private async Task<(bool Success, byte[] Data, string Message)> ReadRegisterWithTimeout(string address, ushort length, int timeoutMs = 5000)
        {
            try
            {
                string adjustedAddress = AdjustAddress(address);

                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    var readTask = Task.Run(() => _modbusClient.Read(adjustedAddress, length), cts.Token);
                    var result = await readTask;

                    _logger.Debug($"寄存器读取完成 - 原地址: {address}, 调整后: {adjustedAddress}, 长度: {length}, 结果: {result.IsSuccess}");

                    return (result.IsSuccess, result.Content, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"寄存器读取异常 - 地址: {address}, 长度: {length}, 异常: {ex.Message}", ex);
                return (false, null, $"寄存器读取异常: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _modbusClient?.ConnectClose();
                _modbusClient = null;
                _disposed = true;
            }
        }
    }
}
