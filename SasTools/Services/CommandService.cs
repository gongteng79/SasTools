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
        public Task<string> ExecuteCommandAsync(CommandType commandType, object parameters = null)
        {
            try
            {
                RequestData requestData;

                // 根据命令类型创建请求数据
                switch (commandType)
                {
                    case CommandType.Subscribe:
                        requestData = DataFactory.CreateData(FunctionType.Subcribe);
                        break;
                    case CommandType.LockMode:
                        requestData = DataFactory.CreateData(FunctionType.LockMode);
                        break;
                    case CommandType.Forward:
                        requestData = DataFactory.CreateData(FunctionType.LockScrewAction);
                        break;
                    case CommandType.Reverse:
                        requestData = DataFactory.CreateData(FunctionType.RemoveScrewAction);
                        break;
                    case CommandType.Stop:
                        requestData = DataFactory.CreateData(FunctionType.Stop);
                        break;
                    case CommandType.ClearTightenInfo:
                        requestData = DataFactory.CreateData(FunctionType.TightenInfoControl);
                        break;
                    case CommandType.StatusQuery:
                        requestData = DataFactory.CreateData(FunctionType.StatusQuery);
                        break;
                    default:
                        throw new ArgumentException($"未知的命令类型: {commandType}");
                }

                // 应用参数 (如果有)
                if (parameters != null)
                {
                    //处理附加参数的逻辑
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
