---
type: "always_apply"
---

var SasToolsDevRules = new
{
    // ===== SasTools特定架构规则 =====
    SasToolsArchitecture = new
    {
        // 事件总线模式 (项目核心通信机制)
        EventBusPattern = new
        {
            EventDefinition = "所有事件必须实现IEvent接口，命名以Event结尾",
            EventHandling = "视图类必须实现IEventHandler<T>接口处理相关事件",
            EventPublishing = "业务逻辑通过_eventBus.Publish()发布事件，禁止直接调用UI更新",
            Example = @"
// 事件定义
public class RefreshMachineState : IEvent
{
    public string DeviceId { get; set; }
    public string Message { get; set; }
}

// 事件处理
public class FatigueTestView : UserControl, IEventHandler<RefreshMachineState>
{
    public void Handle(RefreshMachineState eventData)
    {
        // UI更新逻辑
    }
}"
        },

        // 多设备管理架构
        MultiDeviceManagement = new
        {
            DeviceStorage = "使用ConcurrentDictionary<string, T>存储设备相关数据",
            DeviceIsolation = "每个设备必须有独立的状态机、数据表和UI控件",
            DeviceLifecycle = "设备添加/移除必须通过DeviceManager统一管理",
            Example = @"
private readonly ConcurrentDictionary<string, DeviceTableInfo> _deviceTables;
private readonly DeviceManager _deviceManager;

// 设备创建
_eventBus.Publish(new MultiDeviceCreateEvent(deviceId, device));"
        },

        // 状态机设计模式
        StateMachinePattern = new
        {
            StateDefinition = "使用枚举定义所有测试状态(TestState)",
            StateTransition = "状态转换必须通过TestStateMachine类管理",
            StateLogging = "每次状态变化必须记录日志和发布事件",
            ResourceCleanup = "状态机必须实现IDisposable接口",
            Example = @"
public enum TestState { Idle, Initializing, Forward, Reverse, Error }

public class TestStateMachine : IDisposable
{
    private TestState _state = TestState.Idle;
    private readonly ILog _logger;
    
    private async Task TransitionToState(TestState newState)
    {
        _state = newState;
        _logger.Info($""状态转换: {_state}"");
        PublishStateUpdate();
    }
}"
        }
    },

    // ===== 工业设备通信规则 (适配SasTools) =====
    IndustrialCommunication = new
    {
        // 设备接口抽象
        DeviceAbstraction = new
        {
            InterfaceDesign = "所有设备必须实现IDevice接口",
            CommunicationLayer = "通信层使用ICommunication接口抽象",
            ServiceLayer = "设备服务使用ICommunicationService接口",
            Example = @"
public interface IDevice
{
    bool ConnectServer();
    bool DisconnectServer();
    string ReadData(RequestData data);
    string ExecuteCommand(SasCommandType commandType);
}"
        },

        // 错误处理与恢复
        ErrorHandling = new
        {
            ErrorStrategy = "使用ErrorRecoveryStrategy模式处理设备错误",
            ErrorLogging = "所有通信错误必须记录详细日志",
            RetryMechanism = "实现智能重试机制，最大重试次数为3次",
            Example = @"
private ErrorRecoveryStrategy DetermineRecoveryStrategy(int errorResult, string errorMessage)
{
    if ((errorResult & (1 << 29)) != 0) // 空转错误
    {
        return new ErrorRecoveryStrategy
        {
            Name = ""空转恢复"",
            WaitTime = RecoveryTimes.IDLE_SPIN_RECOVERY,
            RequiresClearTightenInfo = true
        };
    }
    // 其他错误类型...
}"
        },

        // 实时数据处理
        RealTimeData = new
        {
            UIThreadSafety = "UI更新必须使用Control.Invoke或BeginInvoke",
            DataBuffering = "高频数据使用SmartDataCleanup进行智能清理",
            PerformanceOptimization = "避免在UI线程执行耗时操作(>100ms)",
            Example = @"
// UI线程安全更新
if (lblStatus.InvokeRequired)
    lblStatus.BeginInvoke(new Action(() => lblStatus.Text = statusText));

// 数据清理
if (_smartDataCleanup.NeedsCleanup(dataTable))
    _smartDataCleanup.PerformCleanup(dataTable);"
        }
    },

    // ===== 代码质量与风格规则 =====
    CodeQualityRules = new
    {
        // 命名规范 (基于项目实际风格)
        NamingConventions = new
        {
            PrivateFields = "私有字段使用下划线前缀 (_eventBus, _logger, _deviceManager)",
            Controls = "AntdUI控件使用前缀+PascalCase (btnStart, lblStatus, tableDevice)",
            Constants = "常量使用全大写+下划线 (DEFAULT_DELAY, MAX_RETRIES)",
            Methods = "方法名使用PascalCase，异步方法以Async结尾",
            Events = "事件类以Event结尾 (RefreshMachineState, CounterUpdateEvent)",
            Example = @"
private readonly IEventBus _eventBus;
private readonly ILog _logger = LogManager.GetLogger(typeof(FatigueTestView));
private const int DEFAULT_POLLING_INTERVAL = 50;

public async Task ConnectDeviceAsync(string deviceId)
{
    // 异步连接逻辑
}"
        },

        // 注释规范 (支持中文)
        CommentingStandards = new
        {
            ClassComments = "类必须有中文注释说明用途和职责",
            MethodComments = "复杂方法必须有中文注释说明参数和返回值",
            RegionUsage = "使用#region组织代码块 (私有字段、事件处理、资源清理等)",
            Example = @"
/// <summary>
/// 疲劳测试视图 - 负责多设备疲劳测试的UI交互和状态管理
/// </summary>
public partial class FatigueTestView : UserControl
{
    #region 私有字段
    private readonly ILog _logger = LogManager.GetLogger(typeof(FatigueTestView));
    #endregion

    /// <summary>
    /// 创建设备数据表
    /// </summary>
    /// <param name=""deviceId"">设备ID</param>
    /// <returns>创建的数据表</returns>
    private DataTable CreateDeviceTable(string deviceId)
    {
        // 实现逻辑
    }
}"
        },

        // 异常处理模式
        ExceptionHandling = new
        {
            TryCatchPattern = "所有可能抛异常的操作必须用try-catch包装",
            LoggingRequirement = "异常必须记录到日志，包含详细错误信息",
            UserFeedback = "用户界面相关异常需要友好的错误提示",
            Example = @"
try
{
    var result = await _deviceManager.ConnectDevice(deviceId);
    _logger.Info($""设备连接成功: {deviceId}"");
}
catch (Exception ex)
{
    _logger.Error($""设备连接失败: {deviceId}, 错误: {ex.Message}"", ex);
    AntdUI.Message.error(this, ""设备连接失败，请检查网络连接"");
}"
        }
    },

    // ===== 资源管理与性能规则 =====
    ResourceManagement = new
    {
        // IDisposable模式 (项目已广泛使用)
        DisposablePattern = new
        {
            Implementation = "所有管理资源的类必须实现IDisposable接口",
            DisposeOrder = "按照创建顺序的逆序释放资源",
            EventUnsubscription = "Dispose中必须取消所有事件订阅",
            Example = @"
public void Dispose()
{
    if (!_disposed)
    {
        // 取消事件订阅
        _eventBus?.Unsubscribe<RefreshMachineState>(this);
        
        // 释放状态机
        _stateMachine?.Dispose();
        
        // 清理数据表
        foreach (var table in _deviceTables.Values)
        {
            table.Dispose();
        }
        
        _disposed = true;
    }
}"
        },

        // 内存管理
        MemoryManagement = new
        {
            DataTableCleanup = "使用SmartDataCleanup自动清理过期数据",
            TaskCancellation = "长时间运行的Task必须支持CancellationToken",
            WeakReferences = "大对象集合考虑使用WeakReference",
            Example = @"
// 任务取消支持
private CancellationTokenSource _cancellationTokenSource;

public async Task StartTestAsync()
{
    _cancellationTokenSource = new CancellationTokenSource();
    await RunTestLoopAsync(_cancellationTokenSource.Token);
}"
        }
    },

    // ===== 测试与维护规则 =====
    TestingAndMaintenance = new
    {
        // 渐进式测试策略 (适合初级工程师)
        TestingStrategy = new
        {
            UnitTesting = "优先为业务逻辑类编写单元测试 (TestStateMachine, DeviceManager)",
            IntegrationTesting = "设备通信相关功能需要集成测试",
            UITesting = "复杂UI交互需要手动测试用例",
            TestNaming = "测试方法使用中文命名，清晰描述测试场景"
        },

        // 代码优化优先级 (基于用户偏好)
        OptimizationPriority = new
        {
            Priority1 = "内存管理/资源清理 - 修复内存泄漏",
            Priority2 = "性能优化/UI更新 - 减少UI冻结",
            Priority3 = "代码质量/耦合度 - 提高可维护性",
            Priority4 = "测试覆盖率 - 保证代码质量",
            Priority5 = "错误处理标准化 - 统一异常处理"
        },

        // 开发流程
        DevelopmentWorkflow = new
        {
            CodeReview = "代码修改前必须先分析影响范围",
            StepByStep = "复杂修改分步骤进行，每步验证功能正常",
            BackupStrategy = "重要修改前备份相关文件",
            Documentation = "修改后更新相关注释和文档"
        }
    },

    // ===== SasTools特定最佳实践 =====
    SasToolsBestPractices = new
    {
        // 设备管理最佳实践
        DeviceManagement = new
        {
            ConnectionManagement = "设备连接状态只在DeviceManager中管理",
            DeviceIsolation = "每个设备保持独立的状态、数据和UI",
            ErrorIsolation = "单个设备错误不应影响其他设备",
            Example = @"
// 设备隔离示例
private void HandleDeviceError(string deviceId, Exception ex)
{
    // 只影响当前设备
    if (_deviceTables.TryGetValue(deviceId, out var deviceTable))
    {
        deviceTable.CurrentStatus = MachineStatusType.Error;
        deviceTable.StatusMessage = ex.Message;
    }
    
    // 不影响其他设备的运行
}"
        },

        // UI性能优化
        UIPerformance = new
        {
            LazyLoading = "设备表格按需创建，避免预创建所有UI",
            SelectiveRefresh = "只刷新当前可见的设备UI",
            BatchOperations = "批量UI更新使用BeginUpdate/EndUpdate",
            Example = @"
// 延迟加载设备表格
private DeviceTableInfo GetOrCreateDeviceTable(string deviceId)
{
    if (!_deviceTables.TryGetValue(deviceId, out var tableInfo))
    {
        tableInfo = CreateDeviceTable(deviceId);
        _deviceTables.TryAdd(deviceId, tableInfo);
    }
    return tableInfo;
}"
        }
    }
};