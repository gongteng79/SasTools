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
                    return new RequestData { request = 102, sequence = 0, slaveId = 1, screw = 0 };
                case FunctionType.IdleRun:
                    return new RequestData { request = 118, slaveId = 1, velocity = 20, time = 0, angle = 0 };
                case FunctionType.RemoveScrew:
                    return new RequestData { request = 112, slaveId = 1, torque = 0.1, velocity = 500, time = 0, angle = 0 };
                case FunctionType.ScrewParameters:
                    return new RequestData
                    {
                        request = 113,
                        slaveId = 1,
                        screwId = 1,
                        torqueCompensation = 0.000,
                        torqueValidTime = 50,
                        torqueFilterTime = 5,
                        screwCount = 4,
                        velocityTarget = 0.0,
                        velocityMin = 0.0,
                        velocityMax = 0.0,
                        torqueTarget = 0.000,
                        torqueMin = 0.000,
                        torqueMax = 0.000,
                        angleTarget = 0.0,
                        angleMin = 0.0,
                        angleMax = 0.0,
                        stepCount = 2,
                        stepValidStart = 0,
                        steps = new[]
                        {
                        new Step { Index = 1, Velocity = 500.0, Torque = 0.015, Angle = 3600.0, Time = 2000, VelocityFrom = 0.0, VelocityTo = 0.0, Dir = 1, Es = 0, OkIf = new[] { 2, 8 } },
                        new Step { Index = 2, Velocity = 60.0, Torque = 0.049, Angle = 3600.0, Time = 2000, VelocityFrom = 0.0, VelocityTo = 0.0, Dir = 1, Es = 0, OkIf = new[] { 2 } }
                    }
                    };
                case FunctionType.ProductParameters:
                    return new RequestData { request = 130, slaveId = 1, productId = 1, productName = "Product1", screw = 1 };
                case FunctionType.ControlIdleRun:
                    return new RequestData { request = 114, slaveId = 1, velocity = 500, time = 0, angle = 0 };
                case FunctionType.RemoveScrewAction:
                    return new RequestData { request = 115, slaveId = 1, torque = 0.1, velocity = 500, time = 0, angle = 0 };
                case FunctionType.LockScrewAction:
                    return new RequestData { request = 116, slaveId = 1, screwId = 1 };
                case FunctionType.TorqueTest:
                    return new RequestData { request = 117, slaveId = 1, currentPercent = 20, torqueValidTime = 100, velocity = 100 };
                case FunctionType.Stop:
                    return new RequestData { request = 118, slaveId = 1 };
                case FunctionType.StatusQuery:
                    return new RequestData { request = 119, slaveId = 1 };
                case FunctionType.ProductInfoQuery:
                    return new RequestData { request = 120, slaveId = 1 };
                case FunctionType.TorqueCalibration:
                    return new RequestData { request = 121, slaveId = 1 };
                case FunctionType.MotorSelfTest:
                    return new RequestData { request = 122, slaveId = 1, ctrl = 0 };
                case FunctionType.LockMode:
                    return new RequestData { request = 123, slaveId = 1, productId = 1, screwId = 1 };
                case FunctionType.PowerControl:
                    return new RequestData { request = 124, slaveId = 1, powerEnable = 1 };
                case FunctionType.TightenInfoControl:
                    return new RequestData { request = 125, slaveId = 1, tightenClear = 0 };
                case FunctionType.WriteBarcode:
                    return new RequestData { request = 126, slaveId = 1, barCode = 20025954 };
                default:
                    throw new ArgumentOutOfRangeException(nameof(functionType), functionType, null);
            }
        }
    }
}
