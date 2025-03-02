using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Common
{
    public enum FunctionType
    {
        LockScrew,
        IdleRun,
        RemoveScrew,
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