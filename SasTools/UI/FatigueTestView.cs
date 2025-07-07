using log4net;
using SasTools.Interface;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WpFramework.EventBus;
using SasTools.Events;
using SasTools.Services;
using System.Collections.Generic;
using SasTools.Domain;
using static SasTools.Domain.MemoryMonitor;

namespace SasTools.UI
{
    public partial class FatigueTestView : UserControl, IEventHandler<RefreshMachineState>, IEventHandler<CounterUpdateEvent>, IEventHandler<MultiDeviceCreateEvent>, IEventHandler<MultiDeviceRemoveEvent>
    {
        #region 私有字段
        private readonly ILog _logger = LogManager.GetLogger(typeof(FatigueTestView));
        private readonly FatigueParams _parameter;
        private readonly IEventBus _eventBus;
        private readonly DataTable _dataTable;
        private bool _isTestRunning = false;
        private DeviceManager _deviceManager;
        private Dictionary<string, DeviceTableInfo> _deviceTables;
        private SmartDataCleanup _smartDataCleanup;
        private string _currentSelectedDevice;

        // 设备管理常量
        private const int MAX_DEVICE_TABLES = 10; // 最大设备表格数量

        // UI更新相关常量
        private const int UI_UPDATE_BATCH_INTERVAL_MS = 50;           // UI批处理更新间隔（毫秒）
        private const int MEMORY_WARNING_THRESHOLD_MB = 400;          // 内存警告阈值（MB）
        private const int MEMORY_CRITICAL_THRESHOLD_MB = 600;         // 内存危险阈值（MB）
        private const int MEMORY_MONITOR_INTERVAL_SECONDS = 30;       // 内存监控检查间隔（秒）

        // 数据清理相关常量
        private const int DATA_CLEANUP_MAX_ROWS = 50;                 // 数据清理最大行数
        private const int DATA_CLEANUP_TARGET_ROWS = 30;              // 数据清理目标行数
        private const int DATA_CLEANUP_CHECK_INTERVAL_SECONDS = 10;   // 数据清理检查间隔（秒）
        private const int ERROR_DATA_RETENTION_MINUTES = 30;          // 错误数据保留时间（分钟）

        // 时间格式常量
        private const string TIME_FORMAT_FULL = "yyyy-MM-dd HH:mm:ss"; // 完整时间格式

        // 状态文本常量
        private const string STATUS_ERROR = "ERROR";                  // 错误状态文本
        private const string STATUS_NORMAL = "NORMAL";                // 正常状态文本

        // 错误消息常量
        private const string ERROR_MSG_NO_DEVICE_SELECTED = "请先选择要操作的设备";
        private const string ERROR_MSG_DEVICE_NOT_CONNECTED = "选中的设备未连接或未初始化";
        private const string ERROR_MSG_DEVICE_NOT_INITIALIZED = "选中的设备未连接或未初始化";

        // 设备状态相关常量
        private const string STATUS_TEXT_RUNNING = "运行中";
        private const string STATUS_TEXT_IDLE = "空闲";
        private const string LOG_MESSAGE_TEST_STARTED = "测试已开始运行";
        private const string LOG_MESSAGE_TEST_STOPPED = "测试已停止运行";
        private const string LOG_STATE_SYSTEM_START = "系统启动";
        private const string LOG_STATE_SYSTEM_STOP = "系统停止";
        private const string LOG_STATE_SYSTEM_RESET = "系统重置";

        private HashSet<string> _runningDevices = new HashSet<string>(); // 跟踪正在运行的设备
        private Dictionary<string, DateTime> lastCleanupTime = new Dictionary<string, DateTime>();
        private readonly TimeSpan cleanupInterval = TimeSpan.FromSeconds(DATA_CLEANUP_CHECK_INTERVAL_SECONDS);
        private UIUpdateBatcher _uiUpdateBatcher;// UI更新批处理器
        private MemoryMonitor _memoryMonitor;
        private CancellationTokenSource _backgroundTasksCancellationTokenSource;// 添加取消令牌源用于管理后台任务

        #endregion

        #region 构造函数
        public FatigueTestView(IEventBus eventBus, DeviceManager deviceManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));

            // 初始化取消令牌源
            _backgroundTasksCancellationTokenSource = new CancellationTokenSource();

            // 订阅原有事件（保持向后兼容）
            _eventBus.Subscribe<RefreshMachineState>(this);
            _eventBus.Subscribe<CounterUpdateEvent>(this);
            _eventBus.Subscribe<MultiDeviceCreateEvent>(this);
            _eventBus.Subscribe<MultiDeviceRemoveEvent>(this);

            // 订阅DeviceManager的设备选择变化事件
            _deviceManager.DeviceSelectionChanged += OnDeviceManagerSelectionChanged;
            // 订阅设备状态变化事件
            _deviceManager.DeviceStatusChanged += OnDeviceStatusChanged;

            _parameter = new FatigueParams();
            _dataTable = new DataTable();
            _smartDataCleanup = new SmartDataCleanup();
            _uiUpdateBatcher = new UIUpdateBatcher(this, UI_UPDATE_BATCH_INTERVAL_MS);

            // 初始化内存监控  400MB警告，600MB临界
            _memoryMonitor = new MemoryMonitor(MEMORY_WARNING_THRESHOLD_MB, MEMORY_CRITICAL_THRESHOLD_MB);
            _memoryMonitor.MemoryWarning += OnMemoryWarning;

            InitializeComponent();
            InitializeDataTable();
            InitialInput();
            UpdateUIState(false);
            InitializeDeviceStatusBar();
            InitializeDeviceStatusBarEvents();
        }
        #endregion

        #region 初始化方法
        //将错误信息文字设置为红色
        private void ConfigureTableRowStyle(AntdUI.Table table)
        {
            table.SetRowStyle += (sender, e) =>
            {
                try
                {
                    // 调试信息
                    System.Diagnostics.Debug.WriteLine($"SetRowStyle triggered, Record type: {e.Record?.GetType().Name}");

                    // 尝试多种类型转换
                    DataRow dataRow = null;

                    if (e.Record is DataRowView rowView)
                    {
                        dataRow = rowView.Row;
                    }
                    else if (e.Record is DataRow row)
                    {
                        dataRow = row;
                    }

                    if (dataRow != null && dataRow.Table.Columns.Contains("类型"))
                    {
                        var rowType = dataRow["类型"]?.ToString();
                        System.Diagnostics.Debug.WriteLine($"Row type value: {rowType}");

                        if (rowType == STATUS_ERROR)
                        {
                            System.Diagnostics.Debug.WriteLine("Applying red color style");
                            // 返回红色文字样式
                            return new AntdUI.Table.CellStyleInfo
                            {
                                ForeColor = Color.Red
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SetRowStyle error: {ex.Message}");
                }

                return null;
            };
        }

        //初始化数据表
        private void InitializeDeviceDataTable(DeviceTableInfo deviceTableInfo)
        {
            deviceTableInfo.DataSource = new DataTable();
            deviceTableInfo.DataSource.Columns.Add("时间", typeof(string));
            deviceTableInfo.DataSource.Columns.Add("设备", typeof(string));
            deviceTableInfo.DataSource.Columns.Add("状态", typeof(string));
            deviceTableInfo.DataSource.Columns.Add("信息", typeof(string));
            deviceTableInfo.DataSource.Columns.Add("类型", typeof(string));
        }

        //初始化表格的UI控件
        private void InitializeDeviceTableControls(DeviceTableInfo deviceTableInfo)
        {
            deviceTableInfo.Table = new AntdUI.Table
            {
                Dock = DockStyle.Fill,
                DataSource = deviceTableInfo.DataSource
            };

            // 配置行样式
            ConfigureTableRowStyle(deviceTableInfo.Table);

            deviceTableInfo.Container = new AntdUI.Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            deviceTableInfo.Container.Controls.Add(deviceTableInfo.Table);
        }

        //隐藏当前显示的设备表格
        private void HideCurrentDeviceTable()
        {
            if (!string.IsNullOrEmpty(_currentSelectedDevice) && _deviceTables.ContainsKey(_currentSelectedDevice))
            {
                _deviceTables[_currentSelectedDevice].Container.Visible = false;
                _deviceTables[_currentSelectedDevice].IsVisible = false;
            }
        }

        //刷新设备表格的数据显示
        private void RefreshDeviceTableDisplay(DeviceTableInfo deviceTable)
        {
            if (deviceTable.Table == null || deviceTable.DataSource == null)
                return;

            // 强制重新绑定DataSource以确保显示同步
            var currentDataSource = deviceTable.DataSource;
            deviceTable.Table.DataSource = null;
            deviceTable.Table.DataSource = currentDataSource;

            // 滚动到最新记录
            if (currentDataSource.Rows.Count > 0)
            {
                int lastIndex = currentDataSource.Rows.Count - 1;
                deviceTable.Table.SelectedIndex = lastIndex;
                deviceTable.Table.ScrollLine(lastIndex);
            }

            // 使用批处理器进行表格刷新
            _uiUpdateBatcher.QueueUpdate(new StatusUpdateAction(() =>
            {
                if (deviceTable.Table != null && !deviceTable.Table.IsDisposed)
                {
                    deviceTable.Table.Invalidate();
                    deviceTable.Table.Refresh();
                }
            }));
        }

        //清理面板中的提示标签
        private void ClearPanelHintLabels()
        {
            foreach (Control control in panelCurrentTable.Controls)
            {
                if (control is Label)
                {
                    control.Visible = false;
                }
            }
        }
        private void InitializeDataTable()
        {
            // 初始化设备表格字典
            _deviceTables = new Dictionary<string, DeviceTableInfo>();
            _currentSelectedDevice = null;

            // 清空容器
            panelCurrentTable.Controls.Clear();

            // 可以添加一个提示标签   
            var lblHint = new Label
            {
                Text = "请先连接设备",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            panelCurrentTable.Controls.Add(lblHint);
        }

        //初始化设备状态栏事件
        private void InitializeDeviceStatusBarEvents()
        {
            // 订阅设备状态栏的设备选择事件
            if (deviceStatusBar1 != null)
            {
                deviceStatusBar1.DeviceSelected += OnDeviceStatusBarSelected;
            }
        }

        //创建设备数据表格
        private DeviceTableInfo CreateDeviceTable(string deviceId, string deviceName)
        {
            // 检查是否已存在
            if (_deviceTables.ContainsKey(deviceId))
            {
                return _deviceTables[deviceId];
            }

            // 检查设备数量限制
            if (_deviceTables.Count >= MAX_DEVICE_TABLES)
            {
                MessageBox.Show($"设备表格数量已达上限({MAX_DEVICE_TABLES})，请先移除一些设备。");
                return null;
            }

            // 创建新的设备表格信息
            var deviceTableInfo = new DeviceTableInfo
            {
                DeviceId = deviceId,
                DeviceName = deviceName,
                LastAccessTime = DateTime.Now,
                IsVisible = false
            };

            // 初始化数据表和UI控件
            InitializeDeviceDataTable(deviceTableInfo);
            InitializeDeviceTableControls(deviceTableInfo);

            // 添加到字典
            _deviceTables[deviceId] = deviceTableInfo;

            return deviceTableInfo;
        }

        //切换到指定的设备表格
        private void SwitchToDeviceTable(string deviceId)
        {
            try
            {
                // 隐藏当前显示的表格
                HideCurrentDeviceTable();

                // 显示新选中的表格
                if (!string.IsNullOrEmpty(deviceId) && _deviceTables.ContainsKey(deviceId))
                {
                    ShowDeviceTable(deviceId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"切换设备表格失败: {ex.Message}", ex);
            }
        }

        //显示指定的设备表格
        private void ShowDeviceTable(string deviceId)
        {
            var deviceTable = _deviceTables[deviceId];

            // 确保表格容器已添加到面板
            if (!panelCurrentTable.Controls.Contains(deviceTable.Container))
            {
                panelCurrentTable.Controls.Add(deviceTable.Container);
            }

            // 显示表格并更新状态
            deviceTable.Container.Visible = true;
            deviceTable.IsVisible = true;
            deviceTable.LastAccessTime = DateTime.Now;

            // 刷新表格显示
            RefreshDeviceTableDisplay(deviceTable);

            // 更新选中状态和UI
            UpdateSelectedDeviceState(deviceId);
        }

        //更新选中设备的状态和UI
        private void UpdateSelectedDeviceState(string deviceId)
        {
            // 更新当前选中设备
            _currentSelectedDevice = deviceId;

            // 清空提示标签
            ClearPanelHintLabels();

            // 确保DeviceManager的选中设备同步
            if (_deviceManager.GetSelectedDeviceId() != deviceId)
            {
                _deviceManager.SelectDevice(deviceId);
            }

            // 更新UI状态
            UpdateUIForDevice(deviceId);
        }

        //移除设备表格
        public void RemoveDeviceTable(string deviceId)
        {
            if (_deviceTables.ContainsKey(deviceId))
            {
                var deviceTable = _deviceTables[deviceId];

                //从UI中移除
                if (panelCurrentTable.Controls.Contains(deviceTable.Container))
                {
                    panelCurrentTable.Controls.Remove(deviceTable.Container);
                }

                //释放资源
                deviceTable.Dispose();

                //从字典中移除
                _deviceTables.Remove(deviceId);

                // 如果移除的是当前选中设备，需要切换到其他设备或显示提示
                if (_currentSelectedDevice == deviceId)
                {
                    _currentSelectedDevice = null;

                    // 如果还有其他设备，切换到第一个
                    if (_deviceTables.Count > 0)
                    {
                        var firstDevice = _deviceTables.Keys.First();
                        SwitchToDeviceTable(firstDevice);
                    }
                    else
                    {
                        // 显示提示标签
                        ShowNoDeviceHint();
                    }
                }
            }
        }

        //显示无设备提示
        private void ShowNoDeviceHint()
        {
            panelCurrentTable.Controls.Clear();

            var lblHint = new Label
            {
                Text = "请先连接设备",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("微软雅黑", 12)
            };
            panelCurrentTable.Controls.Add(lblHint);
        }

        //初始化参数UI界面
        private void InitialInput()
        {
            InitializeDefaultValues();
            LoadParametersAsync();
        }

        //使用默认参数值初始化UI控件
        private void InitializeDefaultValues()
        {
            var defaultParameters = new TestParameters
            {
                ForwardDelay = _parameter.ForwardDelay,
                ReverseDelay = _parameter.ReverseDelay,
                Timeout = _parameter.Timeout,
                MaxCycles = _parameter.MaxCycles,
                MaxFailures = _parameter.MaxFailures
            };

            UpdateUIParameters(defaultParameters);
        }

        //异步加载保存的参数并更新UI
        private void LoadParametersAsync()
        {
            Task.Run(async () =>
            {
                try
                {
                    var parameters = await _parameter.LoadParameterAsync();

                    // 检查取消令牌和控件状态
                    if (_backgroundTasksCancellationTokenSource?.Token.IsCancellationRequested == true ||
                        this.IsDisposed || !this.IsHandleCreated)
                        return;

                    // 在UI线程中更新控件
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // 再次检查控件状态
                            if (this.IsDisposed) return;

                            // 更新UI和内存参数
                            UpdateUIParameters(parameters);
                            UpdateInMemoryParameters(parameters);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"更新UI参数失败: {ex.Message}", ex);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    _logger.Error($"加载参数失败: {ex.Message}", ex);
                }
            }, _backgroundTasksCancellationTokenSource.Token);
        }

        //统一更新UI控件的参数值
        private void UpdateUIParameters(TestParameters parameters)
        {
            this.txtForwardDelay.Value = parameters.ForwardDelay;
            this.txtReverseDelay.Value = parameters.ReverseDelay;
            this.txtRotationTimes.Value = parameters.Timeout;
            this.inputNumber1.Value = parameters.MaxCycles;
            this.inputNumber2.Value = parameters.MaxFailures;
        }

        //统一更新内存中的参数值
        private void UpdateInMemoryParameters(TestParameters parameters)
        {
            _parameter.ForwardDelay = parameters.ForwardDelay;
            _parameter.ReverseDelay = parameters.ReverseDelay;
            _parameter.Timeout = parameters.Timeout;
            _parameter.MaxCycles = parameters.MaxCycles;
            _parameter.MaxFailures = parameters.MaxFailures;
        }

        //更新设备的测试状态
        private void UpdateDeviceTestState(string deviceId, bool isRunning, MachineStatusType status, string statusText, AntdUI.TState badgeState)
        {
            if (_deviceTables.ContainsKey(deviceId))
            {
                _deviceTables[deviceId].IsTestRunning = isRunning;
                _deviceTables[deviceId].CurrentStatus = status;
                _deviceTables[deviceId].StatusText = statusText;
                _deviceTables[deviceId].BadgeState = badgeState;
            }

            // 更新DeviceManager状态
            _deviceManager.SetDeviceTestState(deviceId, isRunning);

            // 更新UI状态
            UpdateUIForDevice(deviceId);
        }

        //添加设备到运行列表并记录日志
        private void AddDeviceToRunningListAndLog(string deviceId, string logState, string logMessage, MachineStatusType status)
        {
            _runningDevices.Add(deviceId);
            AddLogMessage(deviceId, logState, $"设备 {deviceId} {logMessage}", status);
        }

        //从运行列表移除设备并记录日志
        private void RemoveDeviceFromRunningListAndLog(string deviceId, string logState, string logMessage, MachineStatusType status)
        {
            _runningDevices.Remove(deviceId);
            AddLogMessage(deviceId, logState, $"设备 {deviceId} {logMessage}", status);
        }

        //重置设备数据表格和计数器
        private void ResetDeviceDataAndCounters(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId) && _deviceTables.ContainsKey(deviceId))
            {
                var deviceTable = _deviceTables[deviceId];

                // 清空数据表
                deviceTable.DataSource.Clear();

                // 重置计数器
                deviceTable.UpdateCounters(0, 0, 0);

                // 刷新表格显示
                if (deviceTable.IsVisible && deviceTable.Table != null)
                {
                    deviceTable.Table.Refresh();
                }
            }
        }

        //重置UI显示状态
        private void ResetUIDisplay()
        {
            // 重置UI显示
            this.badge1.State = AntdUI.TState.Default;
            this.badge1.Text = STATUS_TEXT_IDLE;
            this.divider2.Text = "系统已复位";

            // 重置计数器显示
            this.input1.Text = "0";
            this.input2.Text = "0";
            this.input3.Text = "0";
        }
        #endregion

        #region 测试控制方法
        //验证当前选中的设备是否可用于操作
        private bool ValidateCurrentDevice(out string selectedDeviceId, bool showErrorMessage = true)
        {
            selectedDeviceId = _deviceManager.GetSelectedDeviceId();

            if (string.IsNullOrEmpty(selectedDeviceId))
            {
                if (showErrorMessage)
                {
                    AntdUI.Message.error(this.ParentForm, ERROR_MSG_NO_DEVICE_SELECTED);
                }
                return false;
            }

            var currentDevice = _deviceManager.GetDeviceInstance(selectedDeviceId);
            if (currentDevice == null)
            {
                if (showErrorMessage)
                {
                    AntdUI.Message.error(this.ParentForm, ERROR_MSG_DEVICE_NOT_CONNECTED);
                }
                return false;
            }

            return true;
        }

        // 获取当前选中设备的状态机
        private TestStateMachine GetCurrentDeviceStateMachine()
        {
            var selectedDeviceId = _deviceManager.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(selectedDeviceId))
                return null;

            return _deviceManager.GetDeviceStateMachine(selectedDeviceId);
        }

        // 获取当前选中设备的实例
        private IDevice GetCurrentDeviceInstance()
        {
            var selectedDeviceId = _deviceManager.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(selectedDeviceId))
                return null;

            return _deviceManager.GetDeviceInstance(selectedDeviceId);
        }

        //启动当前选中设备的测试
        private void Start()
        {
            try
            {
                if (!ValidateCurrentDevice(out string selectedDeviceId))
                {
                    return;
                }

                var currentStateMachine = GetCurrentDeviceStateMachine();
                if (currentStateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, ERROR_MSG_DEVICE_NOT_INITIALIZED);
                    return;
                }

                // 启动状态机测试
                currentStateMachine.StartTest();

                // 更新设备状态为运行中
                UpdateDeviceTestState(selectedDeviceId, true, MachineStatusType.Forward, STATUS_TEXT_RUNNING, AntdUI.TState.Processing);

                // 添加到运行设备列表并记录日志
                AddDeviceToRunningListAndLog(selectedDeviceId, LOG_STATE_SYSTEM_START, LOG_MESSAGE_TEST_STARTED, MachineStatusType.Forward);
            }
            catch (Exception ex)
            {
                _logger.Error($"启动测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"启动测试发生错误: {ex.Message}");
            }
        }

        //停止当前选中设备的测试
        private void Stop()
        {
            try
            {
                // 使用当前选中的设备ID，确保一致性
                var selectedDeviceId = _currentSelectedDevice ?? _deviceManager.GetSelectedDeviceId();

                if (string.IsNullOrEmpty(selectedDeviceId))
                {
                    AntdUI.Message.error(this.ParentForm, ERROR_MSG_NO_DEVICE_SELECTED);
                    return;
                }

                var currentStateMachine = GetCurrentDeviceStateMachine();
                if (currentStateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, ERROR_MSG_DEVICE_NOT_INITIALIZED);
                    return;
                }

                // 停止状态机测试
                currentStateMachine.StopTest();

                // 更新设备状态为空闲
                UpdateDeviceTestState(selectedDeviceId, false, MachineStatusType.Idle, STATUS_TEXT_IDLE, AntdUI.TState.Default);

                // 从运行设备列表移除并记录日志
                RemoveDeviceFromRunningListAndLog(selectedDeviceId, LOG_STATE_SYSTEM_STOP, LOG_MESSAGE_TEST_STOPPED, MachineStatusType.Idle);
            }
            catch (Exception ex)
            {
                _logger.Error($"停止测试失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"停止测试发生错误: {ex.Message}");
            }
        }

        //重置当前选中设备的测试状态
        private void Reset()
        {
            try
            {
                if (!ValidateCurrentDevice(out string selectedDeviceId))
                {
                    return;
                }

                var currentStateMachine = GetCurrentDeviceStateMachine();
                if (currentStateMachine == null)
                {
                    AntdUI.Message.error(this.ParentForm, ERROR_MSG_DEVICE_NOT_INITIALIZED);
                    return;
                }

                // 使用状态机的完整重置方法
                currentStateMachine.Reset();

                // 更新设备状态为空闲并设置复位消息
                UpdateDeviceTestState(selectedDeviceId, false, MachineStatusType.Idle, STATUS_TEXT_IDLE, AntdUI.TState.Default);

                // 设置复位状态消息
                if (_deviceTables.ContainsKey(selectedDeviceId))
                {
                    _deviceTables[selectedDeviceId].StatusMessage = "系统已复位";
                }

                // 从运行设备列表移除
                _runningDevices.Remove(selectedDeviceId);

                // 重置设备数据和计数器
                ResetDeviceDataAndCounters(selectedDeviceId);

                // 重置UI显示
                ResetUIDisplay();

                // 添加复位日志
                AddLogMessage(selectedDeviceId, LOG_STATE_SYSTEM_RESET, $"设备 {selectedDeviceId} 系统已成功复位，下次启动将从头开始测试", MachineStatusType.Idle);

                // 显示成功消息
                AntdUI.Message.success(this.ParentForm, "系统已成功复位");

                _logger.Info($"用户触发设备 {selectedDeviceId} 系统复位 - 状态机已完全重置");
            }
            catch (Exception ex)
            {
                _logger.Error($"复位系统失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"复位系统发生错误: {ex.Message}");
            }
        }

        private void StopDeviceTest(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId)) return;

                var stateMachine = _deviceManager.GetDeviceStateMachine(deviceId);
                if (stateMachine != null)
                {
                    stateMachine.StopTest();
                    _deviceManager.SetDeviceTestState(deviceId, false);

                    // 更新设备级状态
                    if (_deviceTables.ContainsKey(deviceId))
                    {
                        _deviceTables[deviceId].IsTestRunning = false;
                        _deviceTables[deviceId].CurrentStatus = MachineStatusType.Idle;
                        _deviceTables[deviceId].StatusText = "空闲";
                        _deviceTables[deviceId].BadgeState = AntdUI.TState.Default;
                    }

                    // 从运行设备列表移除
                    _runningDevices.Remove(deviceId);

                    // 更新UI状态
                    if (deviceId == _currentSelectedDevice)
                    {
                        UpdateUIForDevice(deviceId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"停止设备 {deviceId} 测试失败: {ex.Message}", ex);
            }
        }
        #endregion

        #region UI更新方法
        private void UpdateUIState(bool isRunning)
        {
            // 根据测试运行状态更新UI控件状态
            this.button5.Enabled = !isRunning; // 启动按钮
            this.button6.Enabled = isRunning;  // 停止按钮
            this.button7.Enabled = !isRunning; // 复位按钮

            // 测试运行时禁用参数设置
            this.tabPage2.Enabled = !isRunning;

            // 更新状态指示
            if (!isRunning)
            {
                this.badge1.State = AntdUI.TState.Default;
                this.badge1.Text = "空闲";
            }
        }

        // 根据指定设备更新UI状态
        private void UpdateUIForDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId) || !_deviceTables.ContainsKey(deviceId))
            {
                return;
            }

            var deviceTable = _deviceTables[deviceId];
            bool isRunning = deviceTable.IsTestRunning;

            // 更新按钮状态
            this.button5.Enabled = !isRunning; // 启动按钮
            this.button6.Enabled = isRunning;  // 停止按钮
            this.button7.Enabled = !isRunning; // 复位按钮

            // 更新状态指示器
            this.badge1.State = deviceTable.BadgeState;
            this.badge1.Text = deviceTable.StatusText;
            this.divider2.Text = deviceTable.StatusMessage;

            // 更新计数器显示
            this.input1.Text = deviceTable.TotalCycles.ToString();
            this.input2.Text = deviceTable.SuccessCount.ToString();
            this.input3.Text = deviceTable.FailureCount.ToString();

            // 更新参数设置可用性
            this.tabPage2.Enabled = !isRunning;
        }

        //添加日志消息到指定设备的数据表格
        private void AddLogMessage(string targetDeviceId, string state, string message, MachineStatusType statusType)
        {
            if (string.IsNullOrEmpty(targetDeviceId))
            {
                _logger.Warn("AddLogMessage: 设备ID为空，跳过日志添加");
                return;
            }

            try
            {
                // 1. 确保设备表格存在
                var deviceTable = EnsureDeviceTableExists(targetDeviceId);
                if (deviceTable == null) return;

                // 2. 添加数据行到表格
                AddDataRowToTable(deviceTable, targetDeviceId, state, message, statusType);

                // 3. 执行数据清理
                PerformDataCleanupIfNeeded(deviceTable, targetDeviceId);

                // 4. 更新UI
                UpdateTableUIIfVisible(deviceTable, targetDeviceId);
            }
            catch (Exception ex)
            {
                _logger.Error($"添加日志消息失败 - 设备: {targetDeviceId}, 错误: {ex.Message}", ex);
            }
        }

        //确保指定设备的数据表格存在，如果不存在则创建
        private DeviceTableInfo EnsureDeviceTableExists(string deviceId)
        {
            // 如果设备表格已存在，直接返回
            if (_deviceTables.ContainsKey(deviceId))
            {
                return _deviceTables[deviceId];
            }

            // 检查设备表格数量限制
            if (_deviceTables.Count >= MAX_DEVICE_TABLES)
            {
                _logger.Warn($"设备表格数量已达到最大限制 {MAX_DEVICE_TABLES}，无法为设备 {deviceId} 创建新表格");
                return null;
            }

            try
            {
                // 获取设备信息以获取设备名称
                var deviceInfo = _deviceManager?.GetDevice(deviceId);
                string deviceName = deviceInfo?.Name ?? deviceId;

                // 创建新的设备表格
                var deviceTable = CreateDeviceTable(deviceId, deviceName);
                if (deviceTable != null)
                {
                    _deviceTables[deviceId] = deviceTable;
                    _logger.Info($"为设备 {deviceId} 创建了新的数据表格");
                }
                return deviceTable;
            }
            catch (Exception ex)
            {
                _logger.Error($"创建设备 {deviceId} 的数据表格失败: {ex.Message}", ex);
                return null;
            }
        }

        //向设备表格添加数据行
        private void AddDataRowToTable(DeviceTableInfo deviceTable, string deviceId, string state, string message, MachineStatusType statusType)
        {
            var dataSource = deviceTable.DataSource;
            if (dataSource == null)
            {
                _logger.Error($"设备 {deviceId} 的数据源为空");
                return;
            }

            // 线程安全地添加数据行
            lock (dataSource)
            {
                try
                {
                    var newRow = dataSource.NewRow();
                    newRow["时间"] = DateTime.Now.ToString(TIME_FORMAT_FULL);
                    newRow["类型"] = (statusType == MachineStatusType.Error) ? STATUS_ERROR : STATUS_NORMAL;
                    newRow["状态"] = state;
                    newRow["信息"] = message;
                    newRow["设备"] = deviceTable.DeviceName;

                    dataSource.Rows.Add(newRow);
                    deviceTable.LastAccessTime = DateTime.Now;

                    _logger.Debug($"设备 {deviceId} 添加日志: {state} - {message}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"向设备 {deviceId} 表格添加数据行失败: {ex.Message}", ex);
                }
            }
        }

        //根据需要执行数据清理
        private void PerformDataCleanupIfNeeded(DeviceTableInfo deviceTable, string deviceId)
        {
            var dataSource = deviceTable.DataSource;
            if (dataSource == null) return;

            try
            {
                // 检查是否需要执行清理
                bool shouldCheckCleanup = ShouldPerformCleanup(deviceId);

                if (shouldCheckCleanup)
                {
                    lock (dataSource)
                    {
                        _logger.Debug($"开始检查设备 {deviceId} 的数据清理，当前行数：{dataSource.Rows.Count}");

                        // 先执行基于时间的清理
                        _smartDataCleanup.PerformTimeBasedCleanup(dataSource, deviceId);

                        // 再执行基于数量的清理
                        if (_smartDataCleanup.NeedsCleanup(dataSource))
                        {
                            _smartDataCleanup.PerformSmartCleanup(dataSource, deviceId);
                        }

                        // 更新最后清理时间
                        lastCleanupTime[deviceId] = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"设备 {deviceId} 数据清理失败: {ex.Message}", ex);
            }
        }

        //判断是否应该执行数据清理
        private bool ShouldPerformCleanup(string deviceId)
        {
            if (!lastCleanupTime.ContainsKey(deviceId))
            {
                return true; // 首次清理
            }

            return DateTime.Now - lastCleanupTime[deviceId] > cleanupInterval;
        }

            //更新可见表格的UI显示
        private void UpdateTableUIIfVisible(DeviceTableInfo deviceTable, string deviceId)
        {
            // 只对当前显示的设备执行滚动和强制刷新
            if (deviceId != _currentSelectedDevice || !deviceTable.IsVisible)
            {
                return;
            }

            var table = deviceTable.Table;
            var dataSource = deviceTable.DataSource;

            if (table == null || dataSource == null || dataSource.Rows.Count == 0)
            {
                return;
            }

            try
            {
                // 滚动到最新记录
                int lastIndex = Math.Max(0, dataSource.Rows.Count - 1);
                if (lastIndex < dataSource.Rows.Count)
                {
                    table.SelectedIndex = lastIndex;
                    table.ScrollLine(lastIndex);

                    // 使用批处理器进行表格刷新
                    _uiUpdateBatcher.QueueUpdate(new StatusUpdateAction(() =>
                    {
                        if (table != null && !table.IsDisposed && deviceTable.IsVisible)
                        {
                            table.Update();
                            table.Refresh();
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"更新设备 {deviceId} 表格UI失败: {ex.Message}", ex);
            }
        }
        #endregion

        #region 事件处理方法
        void IEventHandler<RefreshMachineState>.Handle(RefreshMachineState evt)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            //使用批处理器
            _uiUpdateBatcher.QueueUpdate(new StatusUpdateAction(() =>
            {
                try
                {
                    string targetDeviceId = evt.DeviceId;

                    if (string.IsNullOrEmpty(targetDeviceId))
                    {
                        _logger.Warn("收到没有设备ID的RefreshMachineState事件，已跳过处理");
                        return;
                    }

                    if (_deviceTables.ContainsKey(targetDeviceId) &&
                        _runningDevices.Contains(targetDeviceId))
                    {
                        AddLogMessage(targetDeviceId, evt.Status, evt.Message, evt.StatusType);

                        var deviceTable = _deviceTables[targetDeviceId];
                        deviceTable.CurrentStatus = evt.StatusType;
                        deviceTable.StatusMessage = evt.Message;

                        if (targetDeviceId == _currentSelectedDevice)
                        {
                            UpdateUIForDevice(targetDeviceId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"更新状态显示失败: {ex.Message}", ex);
                }
            }));
        }

        //处理计数器更新事件，更新界面显示
        void IEventHandler<CounterUpdateEvent>.Handle(CounterUpdateEvent evt)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 验证事件和设备状态
                    if (!ValidateCounterUpdateEvent(evt, out string targetDeviceId))
                        return;

                    // 更新设备计数器数据
                    UpdateDeviceCounters(targetDeviceId, evt);

                    // 刷新可见设备的UI显示
                    RefreshDeviceUIIfVisible(targetDeviceId);

                    // 检查并处理停止条件
                    CheckAndHandleStopConditions(targetDeviceId, evt);
                }
                catch (Exception ex)
                {
                    _logger.Error($"处理计数器更新事件失败: {ex.Message}", ex);
                }
            }));
        }

        //验证计数器更新事件和设备状态
        private bool ValidateCounterUpdateEvent(CounterUpdateEvent evt, out string targetDeviceId)
        {
            targetDeviceId = evt?.DeviceId;

            // 检查事件是否包含设备ID
            if (string.IsNullOrEmpty(targetDeviceId))
            {
                _logger.Warn("收到没有设备ID的CounterUpdateEvent事件，已跳过处理");
                return false;
            }

            // 检查设备是否存在且正在运行
            if (!_deviceTables.ContainsKey(targetDeviceId))
            {
                _logger.Debug($"设备 {targetDeviceId} 的表格不存在，跳过计数器更新");
                return false;
            }

            if (!_runningDevices.Contains(targetDeviceId))
            {
                _logger.Debug($"设备 {targetDeviceId} 未在运行状态，跳过计数器更新");
                return false;
            }

            return true;
        }

        //更新指定设备的计数器数据
        private void UpdateDeviceCounters(string deviceId, CounterUpdateEvent evt)
        {
            try
            {
                var deviceTable = _deviceTables[deviceId];
                deviceTable.UpdateCounters(evt.TotalCycles, evt.SuccessfulCycles, evt.FailedCycles);

                _logger.Debug($"设备 {deviceId} 计数器已更新 - 总计: {evt.TotalCycles}, 成功: {evt.SuccessfulCycles}, 失败: {evt.FailedCycles}");
            }
            catch (Exception ex)
            {
                _logger.Error($"更新设备 {deviceId} 计数器失败: {ex.Message}", ex);
            }
        }

        //刷新可见设备的UI显示
        private void RefreshDeviceUIIfVisible(string deviceId)
        {
            // 只有当前显示的设备才更新UI显示
            if (deviceId != _currentSelectedDevice)
            {
                return;
            }

            try
            {
                UpdateUIForDevice(deviceId);
                _logger.Debug($"设备 {deviceId} UI显示已刷新");
            }
            catch (Exception ex)
            {
                _logger.Error($"刷新设备 {deviceId} UI显示失败: {ex.Message}", ex);
            }
        }

        //检查并处理设备的停止条件
        private void CheckAndHandleStopConditions(string deviceId, CounterUpdateEvent evt)
        {
            try
            {
                // 检查最大循环次数条件
                if (CheckMaxCyclesCondition(deviceId, evt))
                    return; // 如果已停止，不再检查其他条件

                // 检查最大失败次数条件
                CheckMaxFailuresCondition(deviceId, evt);
            }
            catch (Exception ex)
            {
                _logger.Error($"检查设备 {deviceId} 停止条件失败: {ex.Message}", ex);
            }
        }

        //检查最大循环次数停止条件
        private bool CheckMaxCyclesCondition(string deviceId, CounterUpdateEvent evt)
        {
            if (_parameter.MaxCycles <= 0 || evt.TotalCycles < _parameter.MaxCycles)
                return false;

            try
            {
                StopDeviceTest(deviceId);
                AddLogMessage(deviceId, "测试完成",
                    $"设备 {deviceId} 已达到设定的最大循环次数: {_parameter.MaxCycles}",
                    MachineStatusType.Idle);

                _logger.Info($"设备 {deviceId} 因达到最大循环次数而停止测试");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"停止设备 {deviceId} 测试失败（最大循环次数）: {ex.Message}", ex);
                return false;
            }
        }

        //检查最大失败次数停止条件
        private bool CheckMaxFailuresCondition(string deviceId, CounterUpdateEvent evt)
        {
            int failedCycles = evt.TotalCycles - evt.SuccessfulCycles;

            if (_parameter.MaxFailures <= 0 || failedCycles < _parameter.MaxFailures)
                return false;

            try
            {
                StopDeviceTest(deviceId);
                AddLogMessage(deviceId, "测试停止",
                    $"设备 {deviceId} 已达到设定的最大失败次数: {_parameter.MaxFailures}",
                    MachineStatusType.Error);

                _logger.Info($"设备 {deviceId} 因达到最大失败次数而停止测试");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"停止设备 {deviceId} 测试失败（最大失败次数）: {ex.Message}", ex);
                return false;
            }
        }


        // 修改MultiDeviceCreateEvent处理
        void IEventHandler<MultiDeviceCreateEvent>.Handle(MultiDeviceCreateEvent evt)
        {
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 自动将设备添加到状态栏
                    var deviceInfo = _deviceManager?.GetDevice(evt.DeviceId);
                    if (deviceInfo != null)
                    {
                        deviceStatusBar1.AddDevice(evt.DeviceId, deviceInfo.Name);

                        // 创建设备的测试状态机
                        var stateMachine = new TestStateMachine(evt.DeviceId, evt.Device, _parameter, _eventBus);
                        _deviceManager.SetDeviceStateMachine(evt.DeviceId, stateMachine);

                        // 更新设备状态显示
                        deviceStatusBar1.UpdateDeviceStatus(evt.DeviceId, MachineStatusType.Idle, true);
                        // 创建设备对应的表格
                        var deviceTable = CreateDeviceTable(evt.DeviceId, deviceInfo.Name);
                        if (deviceTable != null)
                        {
                            // 如果这是第一个设备，自动切换到它
                            if (_deviceTables.Count == 1)
                            {
                                SwitchToDeviceTable(evt.DeviceId);
                            }
                        }
                    }

                    AddLogMessage(evt.DeviceId, "多设备初始化", $"设备 {evt.DeviceId} 已成功初始化", MachineStatusType.Idle);
                }
                catch (Exception ex)
                {
                    _logger.Error($"处理多设备创建事件失败: {ex.Message}", ex);
                    AddLogMessage(evt.DeviceId, "设备初始化错误", $"设备 {evt.DeviceId} 初始化失败: {ex.Message}", MachineStatusType. Error );
                }
            }));
        }

        // 处理设备移除事件
        void IEventHandler<MultiDeviceRemoveEvent>.Handle(MultiDeviceRemoveEvent evt)
        {
            _uiUpdateBatcher.QueueUpdate(new StatusUpdateAction(() =>
            {
                try
                {
                    // 从状态栏移除设备
                    deviceStatusBar1.RemoveDevice(evt.DeviceId);

                    // 清理设备相关的状态机
                    _deviceManager.SetDeviceStateMachine(evt.DeviceId, null);

                    // 移除设备对应的表格
                    RemoveDeviceTable(evt.DeviceId);

                    AddLogMessage(evt.DeviceId, "设备移除", $"设备 {evt.DeviceId} 已从系统中移除", MachineStatusType.Idle);
                }
                catch (Exception ex)
                {
                    _logger.Error($"处理设备移除事件失败: {ex.Message}", ex);
                    AddLogMessage(evt.DeviceId, "设备移除错误", $"移除设备 {evt.DeviceId} 时发生错误: {ex.Message}", MachineStatusType.Error);
                }
            }));
        }

        //处理设备状态栏设备选择事件
        private void OnDeviceStatusBarSelected(object sender, string deviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(deviceId) && _deviceTables.ContainsKey(deviceId))
                {
                    // 切换到选中的设备表格
                    SwitchToDeviceTable(deviceId);

                    // 同步更新DeviceManager的选中设备
                    _deviceManager.SelectDevice(deviceId);

                    // 记录日志
                    AddLogMessage(deviceId, "设备切换", $"已切换到设备 {deviceId}", MachineStatusType.Idle);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"处理设备状态栏选择事件失败: {ex.Message}", ex);
            }
        }

        #endregion

        #region 按钮事件处理
        // 电批正转按钮
        private void Button2_Click(object sender, EventArgs e)
        {
            if (!ValidateCurrentDevice(out string selectedDeviceId))
            {
                return;
            }

            var currentDevice = GetCurrentDeviceInstance();
            if (currentDevice == null)
            {
                AntdUI.Message.error(this.ParentForm, ERROR_MSG_DEVICE_NOT_CONNECTED);
                return;
            }

            currentDevice.ExecuteCommand(SasCommandType.Forward);
            AddLogMessage(selectedDeviceId, "手动操作", $"设备 {selectedDeviceId} 执行电批正转", MachineStatusType.Forward);
        }

        // 电批反转按钮
        private void Button3_Click(object sender, EventArgs e)
        {
            if (!ValidateCurrentDevice(out string selectedDeviceId))
            {
                return;
            }

            var currentDevice = GetCurrentDeviceInstance();
            if (currentDevice == null)
            {
                AntdUI.Message.error(this.ParentForm, ERROR_MSG_DEVICE_NOT_CONNECTED);
                return;
            }

            currentDevice.ExecuteCommand(SasCommandType.Reverse);
            AddLogMessage(selectedDeviceId, "手动操作", $"设备 {selectedDeviceId} 执行电批反转", MachineStatusType.Forward);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            this.Reset();
        }

        //保存按钮点击事件处理
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // 收集UI参数
                var parameters = CollectTestParametersFromUI();

                // 验证参数
                if (!ValidateTestParameters(parameters))
                {
                    return;
                }

                // 更新内存中的参数
                UpdateInMemoryParameters(parameters);

                // 异步保存参数
                SaveParametersAsync(parameters);

                // 显示立即反馈消息
                AntdUI.Message.success(this.ParentForm, "参数保存成功");
            }
            catch (Exception ex)
            {
                _logger.Error($"保存参数失败: {ex.Message}", ex);
                AntdUI.Message.error(this.ParentForm, $"保存参数失败: {ex.Message}");
            }
        }

        //从UI控件收集测试参数
        private TestParameters CollectTestParametersFromUI()
        {
            return new TestParameters
            {
                ForwardDelay = (int)this.txtForwardDelay.Value,
                ReverseDelay = (int)this.txtReverseDelay.Value,
                Timeout = (int)this.txtRotationTimes.Value,
                MaxCycles = (int)this.inputNumber1.Value,
                MaxFailures = (int)this.inputNumber2.Value
            };
        }

        //验证测试参数
        private bool ValidateTestParameters(TestParameters parameters)
        {
            if (!_parameter.ValidateParameters(parameters, out string errorMessage))
            {
                AntdUI.Message.error(this.ParentForm, errorMessage);
                return false;
            }
            return true;
        }

        //异步保存参数并处理结果
        private void SaveParametersAsync(TestParameters parameters)
        {
            Task.Run(async () =>
            {
                try
                {
                    // 检查取消令牌
                    if (_backgroundTasksCancellationTokenSource?.Token.IsCancellationRequested == true)
                        return;

                    await _parameter.SaveParameterAsync(parameters);

                    // 检查取消令牌和控件状态
                    if (_backgroundTasksCancellationTokenSource?.Token.IsCancellationRequested == true ||
                        this.IsDisposed || !this.IsHandleCreated)
                        return;

                    // 显示保存成功消息
                    ShowSaveSuccessMessage();
                }
                catch (Exception ex)
                {
                    _logger.Error($"保存参数失败: {ex.Message}", ex);
                    ShowSaveErrorMessage(ex);
                }
            }, _backgroundTasksCancellationTokenSource?.Token ?? CancellationToken.None);
        }

        //显示保存成功消息
        private void ShowSaveSuccessMessage()
        {
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    // 再次检查控件状态
                    if (this.IsDisposed) return;

                    string currentDevice = _currentSelectedDevice ?? _deviceManager.GetSelectedDeviceId();
                    if (!string.IsNullOrEmpty(currentDevice))
                    {
                        AddLogMessage(currentDevice, "参数设置", "参数已成功保存", MachineStatusType.Idle);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"更新保存成功消息失败: {ex.Message}", ex);
                }
            }));
        }

        //显示保存错误消息
        private void ShowSaveErrorMessage(Exception ex)
        {
            // 检查控件状态再显示错误消息
            if (_backgroundTasksCancellationTokenSource?.Token.IsCancellationRequested != true &&
                !this.IsDisposed && this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (!this.IsDisposed)
                        {
                            AntdUI.Message.error(this.ParentForm, $"保存参数失败: {ex.Message}");
                        }
                    }
                    catch (Exception uiEx)
                    {
                        _logger.Error($"显示错误消息失败: {uiEx.Message}", uiEx);
                    }
                }));
            }
        }
        #endregion

        #region DeviceStatusBar集成
        private void InitializeDeviceStatusBar()
        {
            // 订阅DeviceStatusBar事件
            deviceStatusBar1.StartAllClicked += OnStartAllClicked;
            deviceStatusBar1.StopAllClicked += OnStopAllClicked;

            // 同步已连接的设备到状态栏
            _deviceManager.SyncDeviceToStatusBar(deviceStatusBar1);
            // 初始化当前设备UI状态
            var currentDeviceId = _deviceManager.GetSelectedDeviceId();
            UpdateCurrentDeviceUI(currentDeviceId);
        }

        private void OnStartAllClicked(object sender, EventArgs e)
        {
            int startedCount = _deviceManager.StartAllDeviceTests();
            AntdUI.Message.success(this.ParentForm, $"已启动 {startedCount} 个设备的测试");
            _logger.Info($"批量启动测试，成功启动 {startedCount} 个设备");
        }

        private void OnStopAllClicked(object sender, EventArgs e)
        {
            int stoppedCount = _deviceManager.StopAllDeviceTests();
            AntdUI.Message.success(this.ParentForm, $"已停止 {stoppedCount} 个设备的测试");
            _logger.Info($"批量停止测试，成功停止 {stoppedCount} 个设备");
        }
        private void OnDeviceManagerSelectionChanged(object sender, string deviceId)
        {
            _uiUpdateBatcher.QueueUpdate(new StatusUpdateAction(() =>
            {
                try
                {
                    // 更新状态栏选择
                    deviceStatusBar1.SelectDevice(deviceId);

                    // 更新UI状态显示
                    UpdateCurrentDeviceUI(deviceId);

                    _logger.Info($"设备选择切换到: {deviceId}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"处理设备选择变化失败: {ex.Message}", ex);
                }
            }));
        }

        // 处理设备状态变化事件
        private void OnDeviceStatusChanged(object sender, SasTools.Services.DeviceStatusChangedEventArgs e)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // 如果设备在状态栏中，更新其状态
                        if (deviceStatusBar1.GetAllDeviceIds().Contains(e.DeviceId))
                        {
                            if (e.DeviceInfo.IsConnected)
                            {
                                // 设备重新连接，更新为空闲状态
                                deviceStatusBar1.UpdateDeviceStatus(e.DeviceId, MachineStatusType.Idle, true);
                            }
                            else
                            {
                                // 设备断开连接，更新为灰色状态
                                deviceStatusBar1.UpdateDeviceStatus(e.DeviceId, MachineStatusType.Idle, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"处理设备状态变化失败: {ex.Message}", ex);
                    }
                }));
            }
        }
        //内存警告处理方法
        private void OnMemoryWarning(object sender, MemoryWarningEventArgs e)
        {
            _uiUpdateBatcher.QueueUpdate(new StatusUpdateAction(() =>
            {
                try
                {
                    string message = $"内存使用过高: {e.MemoryUsageMB}MB";

                    if (e.Level == MemoryWarningLevel.Critical)
                    {
                        // 触发紧急数据清理
                        PerformEmergencyCleanup();
                        AntdUI.Message.warn(this.ParentForm, $"{message} - 已执行紧急清理");
                    }
                    else
                    {
                        AntdUI.Message.info(this.ParentForm, message);
                    }

                    _logger.Warn($"内存警告: {message}, 级别: {e.Level}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"处理内存警告失败: {ex.Message}", ex);
                }
            }));
        }

        private void PerformEmergencyCleanup()
        {
            try
            {
                _logger.Info("开始执行紧急内存清理");

                foreach (var deviceTable in _deviceTables.Values)
                {
                    var dataSource = deviceTable.DataSource;
                    if (dataSource != null && dataSource.Rows.Count > DATA_CLEANUP_MAX_ROWS)
                    {
                        // 紧急清理：只保留最近50条记录和所有错误记录
                        lock (dataSource)
                        {
                            var rowsToKeep = new List<DataRow>();
                            var errorRows = new List<DataRow>();
                            var normalRows = new List<DataRow>();

                            foreach (DataRow row in dataSource.Rows)
                            {
                                if (row["类型"]?.ToString() == STATUS_ERROR)
                                {
                                    errorRows.Add(row);
                                }
                                else
                                {
                                    normalRows.Add(row);
                                }
                            }

                            // 保留所有错误记录
                            rowsToKeep.AddRange(errorRows);

                            // 保留最近50条正常记录
                            if (normalRows.Count > DATA_CLEANUP_TARGET_ROWS)
                            {
                                rowsToKeep.AddRange(normalRows.Skip(normalRows.Count - DATA_CLEANUP_TARGET_ROWS));
                            }   
                            else
                            {
                                rowsToKeep.AddRange(normalRows);
                            }

                            // 清空并重新添加保留的记录
                            dataSource.Clear();
                            foreach (var row in rowsToKeep)
                            {
                                dataSource.ImportRow(row);
                            }
                        }
                    }
                }

                // 强制垃圾回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                _logger.Info("紧急内存清理完成");
            }
            catch (Exception ex)
            {
                _logger.Error($"紧急内存清理失败: {ex.Message}", ex);
            }
        }

        // 更新当前设备的UI状态显示
        private void UpdateCurrentDeviceUI(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                // 没有选中设备
                this.badge1.State = AntdUI.TState.Default;
                this.badge1.Text = "未选择设备";
                this.divider2.Text = "请选择要测试的设备";
                return;
            }

            var deviceInfo = _deviceManager.GetDevice(deviceId);
            if (deviceInfo == null || !deviceInfo.IsConnected)
            {
                // 设备未连接
                this.badge1.State = AntdUI.TState.Default;
                this.badge1.Text = "设备未连接";
                this.divider2.Text = $"设备 {deviceId} 未连接";
                return;
            }

            // 设备已连接，检查测试状态
            bool isRunning = _deviceManager.IsDeviceTestRunning(deviceId);
            if (isRunning)
            {
                this.badge1.State = AntdUI.TState.Processing;
                this.badge1.Text = "测试中";
                this.divider2.Text = $"设备 {deviceId} 正在测试";
            }
            else
            {
                this.badge1.State = AntdUI.TState.Success;
                this.badge1.Text = "就绪";
                this.divider2.Text = $"设备 {deviceId} 已就绪";
            }
        }
        #endregion

    }
}


