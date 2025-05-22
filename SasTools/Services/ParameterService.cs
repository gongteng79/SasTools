using log4net;
using SasTools.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AntdUI;
using SasTools.Domain;
using Newtonsoft.Json;

namespace SasTools.Services
{
    //参数实现服务类，负责参数的保存、加载和验证
    public class ParameterService : IParameterService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ParameterService));
        private readonly string _configFilePath;

        public ParameterService()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            _configFilePath = Path.Combine(appPath, "Config", "TestParameters.json");

            string configDir = Path.GetDirectoryName(_configFilePath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
        }

        //保存参数方法
        public async Task<bool> SaveParameterAsync(TestParameters parameters)
        {
            try
            {
                //将参数对象序列化为格式化的JSON字符串
                string json = JsonConvert.SerializeObject(parameters, Formatting.Indented);
                //使用异步文件流写入JSON字符串到文件
                using (var stream = new FileStream(_configFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                                  bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    //异步写入JSON字符串到文件
                    await writer.WriteAsync(json);
                }
                _logger.Info("参数保存成功");
                return true;

            }
            catch (Exception ex)
            {
                _logger.Error($"保存参数失败: {ex.Message}");
                return false;
            }
        }

        //加载参数方法
        public async Task<TestParameters> LoadParameterAsync()
        {
            try
            {
                //如果配置文件存在
                if (File.Exists(_configFilePath))
                {
                    //使用异步文件流读取文件内容
                    using (var stream = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                              bufferSize: 4096, useAsync: true))
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        //异步读取文件内容
                        string json = await reader.ReadToEndAsync();
                        //反序列化JSON字符串为参数对象
                        var Parameters = JsonConvert.DeserializeObject<TestParameters>(json);
                        _logger.Info($"参数加载成功:{_configFilePath}");
                        return Parameters;
                    }
                }
                else
                {
                    _logger.Info($"参数文件不存在，返回默认参数: {_configFilePath}");
                    return GetDefaultParameters();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"加载参数失败: {ex.Message}");
                return GetDefaultParameters();
            }
        }

        //参数验证方法
        public bool ValidateParameters(TestParameters parameters,out string errorMessage)
        {
            if (parameters == null)
            {
                errorMessage = "参数不能为空";
                return false;
            }

            //验证各个参数范围
            if (parameters.ForwardDelay < 0 || parameters.ForwardDelay > 60) 
            {
                errorMessage = $"正转启动延时必须在 0-60 秒之间(当前值：{parameters.ForwardDelay})";
                return false;
            }
            if (parameters.ReverseDelay < 0 || parameters.ReverseDelay > 60) 
            {
                errorMessage = $"反转启动延时必须在 0-60 秒之间(当前值：{parameters.ReverseDelay})";
                return false;
            }
            if (parameters.RotationInterval < 0 || parameters.RotationInterval > 60) 
            {
                errorMessage = $"正反转切换间隔必须在 0-60 秒之间(当前值：{parameters.RotationInterval})";
                return false;
            }
            if (parameters.StartupInterval < 0 || parameters.StartupInterval > 60) 
            {
                errorMessage = $"循环启动间隔必须在 0-60 秒之间(当前值：{parameters.StartupInterval})";
                return false;
            }
            if (parameters.Timeout < 0 || parameters.Timeout > 60) 
            {
                errorMessage = $"超时时间必须在 0-60 秒之间(当前值：{parameters.Timeout})";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        //获取默认参数方法
        private TestParameters GetDefaultParameters()
        {
            return new TestParameters
            {
                ForwardDelay = 1.0,
                ReverseDelay = 1.0,
                RotationInterval = 2.0,
                StartupInterval = 2.0,
                Timeout = 10
            };
        }
    }
}
