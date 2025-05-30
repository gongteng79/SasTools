using log4net;
using SasTools.Common;
using SasTools.Domain;
using SasTools.Interface;
using System;
using System.Threading.Tasks;

namespace SasTools.Services
{
    // 命令服务实现 - 基于ISasTest接口
    public class CommandService : ICommandService
    {
        private readonly ISasTest _sasTest;
        private readonly ILog _logger = LogManager.GetLogger(typeof(CommandService));

        // 构造函数
        public CommandService(ISasTest sasTest)
        {
            _sasTest = sasTest ?? throw new ArgumentNullException(nameof(sasTest));
        }

        // 执行命令
        public Task<string> ExecuteCommand(CommandType commandType)
        {
            try
            {
                RequestData requestData;
                RequestParameter parameters = null;

                switch (commandType)
                {
                    case CommandType.Subscribe:
                        parameters = new RequestParameter
                        {
                            Request = 101,
                            Sequence = 123,
                            KeepAlive = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.Subcribe, parameters);
                        break;
                    case CommandType.InputScrewData:
                        parameters = new RequestParameter
                        {
                            Request = 102,
                            Sequence = 123,
                            SlaveId = 1,
                            Screw = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.InputScrewData, parameters);
                        break;
                    case CommandType.Forward:
                        parameters = new RequestParameter
                        {
                            Request = 116,
                            SlaveId = 1,
                            ScrewId = 1
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.LockScrewAction, parameters);
                        break;
                    case CommandType.Reverse:
                        parameters = new RequestParameter
                        {
                            Request = 115,
                            SlaveId = 1,
                            Torque = 0, //最大扭矩
                            Velocity = 500,
                            Time = 5000,//反转转动的时间(5s)
                            Angle = 0
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.RemoveScrewAction, parameters);
                        break;
                    case CommandType.Stop:
                        parameters = new RequestParameter
                        {
                            Request = 118,
                            SlaveId = 1,
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.Stop, parameters);
                        break;
                    case CommandType.ClearTightenInfo:
                        parameters = new RequestParameter
                        {
                            Request = 125,
                            SlaveId = 1,
                            TightenClear = 2
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.TightenInfoControl, parameters);
                        break;
                    case CommandType.StatusQuery:
                        parameters = new RequestParameter
                        {
                            Request = 119,
                            SlaveId = 1,
                        };
                        requestData = DataFactory.CreateRequestCommand(FunctionType.StatusQuery, parameters);
                        break;
                    default:
                        throw new ArgumentException($"未知的命令类型: {commandType}");
                }

                // 应用参数 (如果有)
                if (parameters == null)
                {
                    System.Windows.Forms.MessageBox.Show("parameters 为空！");
                }

                // 发送命令并获取响应
                string response = _sasTest.ReadData(requestData);
                _logger.Info($"执行命令 {commandType}，响应: {response}");

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.Error($"执行命令 {commandType} 失败: {ex.Message}", ex);
                throw;
            }
        }
    }
}
