using log4net;
using SasTools.Domain;
using SasTools.Interface;
using SasTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;

namespace SasTools.UI
{
    public partial class ManualView : UserControl
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ManualView));
        private IParameterService _parameterService;
        private TestParameters _currentParameters;

        public ManualView()
        {
            InitializeComponent();

            // 初始化参数服务
            _parameterService = new ParameterService();

            // 初始化界面事件
            InitializeEvents();
        }

        // 由主窗体调用，传入依赖服务
        public void Initialize(IParameterService parameterService)
        {
            _parameterService = parameterService;

            // 加载当前螺丝规格和参数
            LoadScrewSpecAndParameters();
        }

        private async void LoadScrewSpecAndParameters()
        {
            try
            {
                // 获取当前选中的螺丝规格
                string currentSpec = _parameterService.GetCurrentScrewSpec();

                // 设置下拉框选中项
                ScrewResultselect.Text = currentSpec;

                // 加载对应规格的参数
                await LoadParametersForSpec(currentSpec);
            }
            catch (Exception ex)
            {
                _logger.Error($"加载螺丝规格和参数失败: {ex.Message}", ex);
                MessageBox.Show($"加载参数失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadParametersForSpec(string screwSpec)
        {
            try
            {
                // 加载指定规格的参数
                _currentParameters = await _parameterService.LoadParameterAsync(screwSpec);

                // 更新界面显示
                DisplayParameters(_currentParameters);
            }
            catch (Exception ex)
            {
                _logger.Error($"加载参数失败，螺丝规格: {screwSpec}, 错误: {ex.Message}", ex);
                throw;
            }
        }

        private void DisplayParameters(TestParameters parameters)
        {
            // 显示参数到界面控件
            txtForwardDelay.Text = parameters.ForwardDelay.ToString("F1");
            txtReverseDelay.Text = parameters.ReverseDelay.ToString("F1");
            txtRotationInterval.Text = parameters.RotationInterval.ToString("F1");
            txtStartupInterval.Text = parameters.StartupInterval.ToString("F1");
            txtRotationTimes.Text = parameters.Timeout.ToString();
        }

        private void InitializeEvents()
        {
            // 螺丝规格选择变更事件
            ScrewResultselect.SelectedValueChanged += async (s, e) =>
            {
                try
                {
                    string selectedSpec = ScrewResultselect.Text;
                    if (!string.IsNullOrEmpty(selectedSpec))
                    {
                        // 设置当前选中的螺丝规格
                        await _parameterService.SetCurrentScrewSpec(selectedSpec);

                        // 加载对应规格的参数
                        await LoadParametersForSpec(selectedSpec);

                        _logger.Info($"已切换螺丝规格: {selectedSpec}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"切换螺丝规格失败: {ex.Message}", ex);
                    AntdUI.Message.error(this.ParentForm, $"切换螺丝规格失败: {ex.Message}");
                }
            };

            // 保存按钮点击事件
            Savebutton.Click += async (s, e) =>
            {
                try
                {
                    Savebutton.Loading = true;

                    // 获取界面输入的参数值
                    var parameters = new TestParameters
                    {
                        ForwardDelay = double.Parse(txtForwardDelay.Text),
                        ReverseDelay = double.Parse(txtReverseDelay.Text),
                        RotationInterval = double.Parse(txtRotationInterval.Text),
                        StartupInterval = double.Parse(txtStartupInterval.Text),
                        Timeout = int.Parse(txtRotationTimes.Text)
                    };

                    // 验证参数
                    if (_parameterService.ValidateParameters(parameters, out string errorMessage))
                    {
                        // 获取当前选中的螺丝规格
                        string currentSpec = ScrewResultselect.Text;

                        // 保存参数
                        bool success = await _parameterService.SaveParameterAsync(parameters, currentSpec);

                        if (success)
                        {
                            _currentParameters = parameters;
                            _logger.Info($"参数保存成功，螺丝规格: {currentSpec}");
                            AntdUI.Message.success(this.ParentForm, "参数保存成功！");
                        }
                        else
                        {
                            AntdUI.Message.error(this.ParentForm, "参数保存失败！");
                        }
                    }
                    else
                    {
                        AntdUI.Message.error(this.ParentForm, $"参数无效: {errorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"保存参数失败: {ex.Message}", ex);
                    AntdUI.Message.error(this.ParentForm, $"保存参数失败: {ex.Message}");
                }
                finally
                {
                    Savebutton.Loading = false;
                }
            };

            // 添加手动操作按钮事件
            //此功能暂时还没有
            //button1.Click += (s, e) => { /* 上下气缸手动 */ };
            button2.Click += (s, e) => 
            {
                //点击电批正传按钮执行正转操作
                if (_currentParameters != null)
                {
                     
                }
            };
            button3.Click += (s, e) => 
            {
                //点击电批反转按钮执行反转操作
                if (_currentParameters != null)
                {
                    
                }
            };
        }
    }
}
