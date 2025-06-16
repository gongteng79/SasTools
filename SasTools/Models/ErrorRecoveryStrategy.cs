namespace SasTools.Models
{
    //错误恢复策略配置
    public class ErrorRecoveryStrategy
    {
        //策略名称
        public string Name { get; set; }

        //等待时间（毫秒）
        public int WaitTime { get; set; }

        //是否需要清理拧紧信息
        public bool RequiresClearTightenInfo { get; set; }

        //是否需要重新订阅
        public bool RequiresResubscribe { get; set; }

        //是否需要额外延时
        public bool RequiresExtraDelay { get; set; }
    }
}
