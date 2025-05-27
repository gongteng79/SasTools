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
        private readonly string _configDir;
        private readonly string _currentSpecFilePath;
        private string _currentScrewSpec = "M8"; // 默认螺丝规格

        public ParameterService()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            _configDir = Path.Combine(appPath, "Config", "ScrewParameters");
            _currentSpecFilePath = Path.Combine(appPath, "Config", "CurrentSpec.json");

            // 确保配置目录存在
            if (!Directory.Exists(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }

            // 加载当前选择的螺丝规格
            LoadCurrentScrewSpec();
        }

        // 获取螺丝规格对应的配置文件路径
        private string GetConfigFilePath(string screwSpec)
        {
            return Path.Combine(_configDir, $"{screwSpec}.json");
        }

        // 获取默认配置文件路径
        private string GetDefaultConfigFilePath()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appPath, "Config", "TestParameters.json");
        }

        // 加载当前选择的螺丝规格
        private void LoadCurrentScrewSpec()
        {
            try
            {
                if (File.Exists(_currentSpecFilePath))
                {
                    string json = File.ReadAllText(_currentSpecFilePath);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (data != null && data.ContainsKey("CurrentSpec"))
                    {
                        _currentScrewSpec = data["CurrentSpec"];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"加载当前螺丝规格失败: {ex.Message}");
                // 失败时使用默认值 M8
            }
        }

        //保存参数方法
        public async Task<bool> SaveParameterAsync(TestParameters parameters)
        {
            return await SaveParameterAsync(parameters, _currentScrewSpec);
        }

        //加载参数方法
        public async Task<TestParameters> LoadParameterAsync()
        {
            return await LoadParameterAsync(_currentScrewSpec);
        }

        //保存指定规格的参数
        public async Task<bool> SaveParameterAsync(TestParameters parameters, string screwSpec)
        {
            try
            {
                string configFilePath = GetConfigFilePath(screwSpec);

                //将参数对象序列化为格式化的JSON字符串
                string json = JsonConvert.SerializeObject(parameters, Formatting.Indented);

                //使用异步文件流写入JSON字符串到文件
                using (var stream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                                  bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    //异步写入JSON字符串到文件
                    await writer.WriteAsync(json);
                }

                _logger.Info($"参数保存成功，螺丝规格：{screwSpec}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"保存参数失败，螺丝规格：{screwSpec}, 错误：{ex.Message}");
                return false;
            }
        }

        //加载指定规格的参数
        public async Task<TestParameters> LoadParameterAsync(string screwSpec)
        {
            try
            {
                string configFilePath = GetConfigFilePath(screwSpec);

                // 如果指定规格的配置文件不存在，但默认的配置文件存在
                if (!File.Exists(configFilePath))
                {
                    string defaultFilePath = GetDefaultConfigFilePath();
                    if (File.Exists(defaultFilePath))
                    {
                        configFilePath = defaultFilePath;
                    }
                    else
                    {
                        _logger.Info($"参数文件不存在，返回默认参数：{screwSpec}");
                        return GetDefaultParameters();
                    }
                }

                //使用异步文件流读取文件内容
                using (var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                          bufferSize: 4096, useAsync: true))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    //异步读取文件内容
                    string json = await reader.ReadToEndAsync();
                    //反序列化JSON字符串为参数对象
                    var parameters = JsonConvert.DeserializeObject<TestParameters>(json);
                    _logger.Info($"参数加载成功，螺丝规格：{screwSpec}");
                    return parameters;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"加载参数失败，螺丝规格：{screwSpec}, 错误：{ex.Message}");
                return GetDefaultParameters();
            }
        }

        //参数验证方法
        public bool ValidateParameters(TestParameters parameters, out string errorMessage)
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

        // 获取当前选中的螺丝规格
        public string GetCurrentScrewSpec()
        {
            return _currentScrewSpec;
        }

        // 设置当前选中的螺丝规格
        public async Task<bool> SetCurrentScrewSpec(string screwSpec)
        {
            try
            {
                _currentScrewSpec = screwSpec;

                // 保存当前规格到配置文件
                var data = new Dictionary<string, string> { { "CurrentSpec", screwSpec } };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);

                using (var stream = new FileStream(_currentSpecFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                                  bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(json);
                }

                _logger.Info($"已设置当前螺丝规格: {screwSpec}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"设置当前螺丝规格失败: {ex.Message}");
                return false;
            }
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

