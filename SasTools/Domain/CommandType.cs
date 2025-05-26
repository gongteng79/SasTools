using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    // 命令类型枚举
    public enum CommandType
    {
        Subscribe,       // 订阅
        LockMode,        // 锁付模式
        Forward,         // 正转
        Reverse,         // 反转
        Stop,            // 停止
        ClearTightenInfo, // 清除锁付信息
        StatusQuery       //状态查询
    }
}


