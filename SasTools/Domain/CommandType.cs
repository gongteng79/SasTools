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
        Subscribe,       // 101订阅
        InputScrewData, // 102传入锁付数据
        LockMode,        // 123锁付模式
        Forward,         // 116正转
        Reverse,         // 115反转
        Stop,            // 118停止
        ClearTightenInfo, // 125清除锁付信息
        StatusQuery       //119状态查询
    }
}


