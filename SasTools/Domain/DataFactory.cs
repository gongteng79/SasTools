using SasTools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Domain
{
    public class DataFactory
    {
        public static RequestData CreateData(FunctionType functionType)
        {
            switch (functionType)
            {
                case FunctionType.LockScrew:
                    return new RequestData { Request = 102, Sequence = 0, SlaveId = 1, Screw = 0 };
                case FunctionType.IdleRun:
                    return new RequestData { Request = 118, SlaveId = 1, Velocity = 20, Time = 0, Angle = 0 };
                case FunctionType.RemoveScrew:
                    return new RequestData { Request = 112, SlaveId = 1, Torque = 0.1, Velocity = 500, Time = 0, Angle = 0 };
                case FunctionType.ScrewParameters:
                    return new RequestData
                    {
                        Request = 113,
                        SlaveId = 1,
                        ScrewId = 1,
                        TorqueCompensation = 0.000,
                        TorqueValidTime = 50,
                        TorqueFilterTime = 5,
                        ScrewCount = 4,
                        VelocityTarget = 0.0,
                        VelocityMin = 0.0,
                        VelocityMax = 0.0,
                        TorqueTarget = 0.000,
                        TorqueMin = 0.000,
                        TorqueMax = 0.000,
                        AngleTarget = 0.0,
                        AngleMin = 0.0,
                        AngleMax = 0.0,
                        StepCount = 2,
                        StepValidStart = 0,
                        Steps = new[]
                        {
                        new Step { Index = 1, Velocity = 500.0, Torque = 0.015, Angle = 3600.0, Time = 2000, VelocityFrom = 0.0, VelocityTo = 0.0, Dir = 1, Es = 0, OkIf = new[] { 2, 8 } },
                        new Step { Index = 2, Velocity = 60.0, Torque = 0.049, Angle = 3600.0, Time = 2000, VelocityFrom = 0.0, VelocityTo = 0.0, Dir = 1, Es = 0, OkIf = new[] { 2 } }
                    }
                    };
                case FunctionType.ProductParameters:
                    return new RequestData { Request = 130, SlaveId = 1, ProductId = 1, ProductName = "Product1", Screw = 1 };
                case FunctionType.ControlIdleRun:
                    return new RequestData { Request = 114, SlaveId = 1, Velocity = 500, Time = 0, Angle = 0 };
                case FunctionType.RemoveScrewAction:
                    return new RequestData { Request = 115, SlaveId = 1, Torque = 0.1, Velocity = 500, Time = 0, Angle = 0 };
                case FunctionType.LockScrewAction:
                    return new RequestData { Request = 116, SlaveId = 1, ScrewId = 1 };
                case FunctionType.TorqueTest:
                    return new RequestData { Request = 117, SlaveId = 1, CurrentPercent = 20, TorqueValidTime = 100, Velocity = 100 };
                case FunctionType.Stop:
                    return new RequestData { Request = 118, SlaveId = 1 };
                case FunctionType.StatusQuery:
                    return new RequestData { Request = 119, SlaveId = 1 };
                case FunctionType.ProductInfoQuery:
                    return new RequestData { Request = 120, SlaveId = 1 };
                case FunctionType.TorqueCalibration:
                    return new RequestData { Request = 121, SlaveId = 1 };
                case FunctionType.MotorSelfTest:
                    return new RequestData { Request = 122, SlaveId = 1, Ctrl = 0 };
                case FunctionType.LockMode:
                    return new RequestData { Request = 123, SlaveId = 1, ProductId = 1, ScrewId = 1 };
                case FunctionType.PowerControl:
                    return new RequestData { Request = 124, SlaveId = 1, PowerEnable = 1 };
                case FunctionType.TightenInfoControl:
                    return new RequestData { Request = 125, SlaveId = 1, TightenClear = 0 };
                case FunctionType.WriteBarcode:
                    return new RequestData { Request = 126, SlaveId = 1, BarCode = 20025954 };
                default:
                    throw new ArgumentOutOfRangeException(nameof(functionType), functionType, null);
            }
        }
    }
}
