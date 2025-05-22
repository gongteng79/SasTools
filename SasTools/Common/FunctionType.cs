using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Common
{
    public enum FunctionType
    {
        Subcribe,//订阅
        InputScrewData,//传入锁付数据
        IdleRunParameter,//空转运行参数
        RemoveScrewParameter,//拆螺丝参数
        ScrewParameters,
        ProductParameters,
        ControlIdleRun,
        RemoveScrewAction,
        LockScrewAction,
        TorqueTest,
        Stop,
        StatusQuery,
        ProductInfoQuery,
        TorqueCalibration,
        MotorSelfTest,
        LockMode,
        PowerControl,
        TightenInfoControl,
        WriteBarcode
    }
}