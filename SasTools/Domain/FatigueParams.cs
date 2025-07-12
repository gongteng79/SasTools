using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using SasTools.Interface;

namespace SasTools.Domain
{
    public class FatigueParams : ParameterBase
    {
        // 默认值常量
        private const int DEFAULT_DELAY = 1000;
        private const int DEFAULT_TIMEOUT = 1000;

        // 字段
        private int _forwardDelay = DEFAULT_DELAY;
        private int _reverseDelay = DEFAULT_DELAY;
        private int _timeout = DEFAULT_TIMEOUT;
        private int _maxCycles = 0; // 0表示无限循环
        private int _maxFailures = 0; // 0表示不限制失败次数
        private int _reverseVelocity = 500;  // 默认反转转速
        private int _reverseTime = 1000;     // 默认反转时间
        private readonly ILog _logger;
        private readonly string _configFilePath;

        public FatigueParams()
        {
            _logger = LogManager.GetLogger(typeof(FatigueParams));

            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            _configFilePath = Path.Combine(appPath, "Config", "TestParameters.txt");

            EnsureConfigDirectoryExists();
        }

        // 确保配置目录存在
        private void EnsureConfigDirectoryExists()
        {
            string configDir = Path.GetDirectoryName(_configFilePath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
                _logger.Info($"创建配置目录: {configDir}");
            }
        }

        //正转启动延时(毫秒)
        public int ForwardDelay
        {
            get => _forwardDelay;
            set { if (_forwardDelay != value) _forwardDelay = value; }
        }

        //反转启动延时(毫秒)
        public int ReverseDelay
        {
            get => _reverseDelay;
            set { if (_reverseDelay != value) _reverseDelay = value; }
        }

        //超时设置(毫秒)
        public int Timeout
        {
            get => _timeout;
            set { if (_timeout != value) _timeout = value; }
        }

        // 最大循环次数(0表示无限循环)
        public int MaxCycles
        {
            get => _maxCycles;
            set { if (_maxCycles != value) _maxCycles = value; }
        }

        // 最大失败次数(0表示不限制失败次数)
        public int MaxFailures
        {
            get => _maxFailures;
            set { if (_maxFailures != value) _maxFailures = value; }
        }

        //反转转速(rpm)
        public int ReverseVelocity
        {
            get => _reverseVelocity;
            set => _reverseVelocity = value;
        }

        //反转时间(毫秒)  
        public int ReverseTime
        {
            get => _reverseTime;
            set => _reverseTime = value;
        }

        public override async Task<TestParameters> LoadParameterAsync()
        {
            try
            {
                EnsureConfigDirectoryExists();

                // 创建默认参数对象
                var parameters = CreateDefaultParameters();

                // 如果配置文件不存在，创建一个新文件并保存默认参数
                if (!File.Exists(_configFilePath))
                {
                    _logger.Info($"参数文件不存在，创建默认参数文件: {_configFilePath}");
                    await SaveParameterAsync(parameters);
                    return parameters;
                }

                // 读取并解析配置文件
                Dictionary<string, int> paramDict = await ReadParametersFromFileAsync();

                // 更新参数对象
                UpdateParametersFromDictionary(parameters, paramDict);

                _logger.Info($"参数加载成功: {_configFilePath}");
                return parameters;
            }
            catch (Exception ex)
            {
                _logger.Error($"加载参数失败: {ex.Message}", ex);
                return CreateDefaultParameters();
            }
        }

        private TestParameters CreateDefaultParameters()
        {
            return new TestParameters
            {
                ForwardDelay = _forwardDelay,
                ReverseDelay = _reverseDelay,
                Timeout = _timeout,
                MaxCycles = _maxCycles,
                MaxFailures = _maxFailures,
                ReverseVelocity = _reverseVelocity,
                ReverseTime = _reverseTime
            };
        }

        private async Task<Dictionary<string, int>> ReadParametersFromFileAsync()
        {
            Dictionary<string, int> paramDict = new Dictionary<string, int>();

            using (var stream = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                      bufferSize: 4096, useAsync: true))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;

                // 按行读取参数文件
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int value))
                    {
                        paramDict[parts[0].Trim()] = value;
                    }
                }
            }

            return paramDict;
        }

        private void UpdateParametersFromDictionary(TestParameters parameters, Dictionary<string, int> paramDict)
        {
            if (paramDict.TryGetValue("ForwardDelay", out int forwardDelay))
                parameters.ForwardDelay = forwardDelay;

            if (paramDict.TryGetValue("ReverseDelay", out int reverseDelay))
                parameters.ReverseDelay = reverseDelay;

            if (paramDict.TryGetValue("Timeout", out int timeout))
                parameters.Timeout = timeout;

            if (paramDict.TryGetValue("MaxCycles", out int maxCycles))
                parameters.MaxCycles = maxCycles;

            if (paramDict.TryGetValue("MaxFailures", out int maxFailures))
                parameters.MaxFailures = maxFailures;

            if (paramDict.TryGetValue("ReverseVelocity", out int reverseVelocity))
            {
                parameters.ReverseVelocity = reverseVelocity;
                _reverseVelocity = reverseVelocity;
            }

            if (paramDict.TryGetValue("ReverseTime", out int reverseTime))
            {
                parameters.ReverseTime = reverseTime;
                _reverseTime = reverseTime;
            }

        }

        public override async Task<bool> SaveParameterAsync(TestParameters parameters)
        {
            try
            {
                EnsureConfigDirectoryExists();

                // 准备写入的字符串
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# 疲劳测试参数配置文件");
                sb.AppendLine($"ForwardDelay={parameters.ForwardDelay}");
                sb.AppendLine($"ReverseDelay={parameters.ReverseDelay}");
                sb.AppendLine($"Timeout={parameters.Timeout}");
                sb.AppendLine($"MaxCycles={parameters.MaxCycles}");
                sb.AppendLine($"MaxFailures={parameters.MaxFailures}");
                sb.AppendLine($"ReverseVelocity={parameters.ReverseVelocity}");
                sb.AppendLine($"ReverseTime={parameters.ReverseTime}");

                // 使用异步文件流写入文本到文件
                using (var stream = new FileStream(_configFilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                                  bufferSize: 4096, useAsync: true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(sb.ToString());
                }

                _logger.Info("参数保存成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"保存参数失败: {ex.Message}", ex);
                return false;
            }
        }

        public override bool ValidateParameters(TestParameters parameters, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (parameters == null)
            {
                errorMessage = "参数不能为空";
                return false;
            }

            //验证各个参数范围
            if (parameters.ForwardDelay < 0 || parameters.ForwardDelay > 60000)
            {
                errorMessage = $"正转启动延时必须在 0-60 秒之间(当前值：{parameters.ForwardDelay})";
                return false;
            }
            if (parameters.ReverseDelay < 0 || parameters.ReverseDelay > 60000)
            {
                errorMessage = $"反转启动延时必须在 0-60 秒之间(当前值：{parameters.ReverseDelay})";
                return false;
            }
            if (parameters.Timeout < 0 || parameters.Timeout > 60000)
            {
                errorMessage = $"超时时间必须在 0-60 秒之间(当前值：{parameters.Timeout})";
                return false;
            }
            if (parameters.MaxCycles < 0)
            {
                errorMessage = $"循环次数必须大于等于0(当前值：{parameters.MaxCycles})";
                return false;
            }
            if (parameters.MaxFailures < 0)
            {
                errorMessage = $"最大失败次数必须大于等于0(当前值：{parameters.MaxFailures})";
                return false;
            }
            if (parameters.ReverseVelocity < 100 || parameters.ReverseVelocity > 3000)
            {
                errorMessage = $"反转转速必须在 100-3000 rpm 之间(当前值：{parameters.ReverseVelocity})";
                return false;
            }
            if (parameters.ReverseTime < 100 || parameters.ReverseTime > 10000)
            {
                errorMessage = $"反转时间必须在 0.1-10 秒之间(当前值：{parameters.ReverseTime})";
                return false;
            }

            return true;
        }
    }
}

