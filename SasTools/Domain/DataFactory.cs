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
        // 请求代码常量
        private static class FunctionCodes
        {
            public const int Subscribe = 101;
            public const int InputScrewData = 102;
            public const int IdleRunParameter = 111;
            public const int RemoveScrewParameter = 112;
            public const int ScrewParameters = 113;
            public const int ControlIdleRun = 114;
            public const int RemoveScrewAction = 115;
            public const int LockScrewAction = 116;
            public const int TorqueTest = 117;
            public const int Stop = 118;
            public const int StatusQuery = 119;
            public const int ProductInfoQuery = 120;
            public const int TorqueCalibration = 121;
            public const int MotorSelfTest = 122;
            public const int LockMode = 123;
            public const int PowerControl = 124;
            public const int TightenInfoControl = 125;
            public const int WriteBarcode = 126;
            public const int ProductParameters = 130;
        }

        // 设备ID常量
        private const int DefaultSlaveId = 1;

        // 其他常用参数默认值
        private static class DefaultValues
        {
            public const int Sequence = 0;
            public const int KeepAlive = 1;
            public const int CacheClear = 0;
            public const int DefaultScrew = 0;
            public const int DefaultProductId = 1;
            public const int DefaultVelocity = 500;
            public const int DefaultTime = 0;
            public const double DefaultAngle = 0.0;
            public const double DefaultTorque = 0.1;
            public const int DefaultCurrentPercent = 20;
            public const int DefaultTorqueValidTime = 100;
            public const int DefaultTorqueFilterTime = 5;
            public const int DefaultScrewCount = 4;
            public const int DefaultStepCount = 2;
            public const int DefaultStepValidStart = 0;
            public const int DefaultPowerEnable = 1;
            public const int DefaultTightenClear = 2;
            public const int DefaultCtrl = 0;
            public const int DefaultBarCode = 20025954;
            public const string DefaultProductName = "Product1";
        }

        public static RequestData CreateData(FunctionType functionType)
        {
            switch (functionType)
            {
                case FunctionType.Subcribe:
                    return new RequestData
                    {
                        request = FunctionCodes.Subscribe,
                        sequence = DefaultValues.Sequence,
                        slave_id = DefaultSlaveId,
                        keep_alive = DefaultValues.KeepAlive,
                        cache_clear = DefaultValues.CacheClear
                    };
                case FunctionType.InputScrewData:
                    return new RequestData
                    {
                        request = FunctionCodes.InputScrewData,
                        sequence = DefaultValues.Sequence,
                        slave_id = DefaultSlaveId,
                        screw = DefaultValues.DefaultScrew
                    };
                case FunctionType.IdleRunParameter:
                    return new RequestData
                    {
                        request = FunctionCodes.IdleRunParameter,
                        slave_id = DefaultSlaveId,
                        velocity = DefaultValues.DefaultVelocity,
                        time = DefaultValues.DefaultTime,
                        angle = DefaultValues.DefaultAngle
                    };
                case FunctionType.RemoveScrewParameter:
                    return new RequestData
                    {
                        request = FunctionCodes.RemoveScrewParameter,
                        slave_id = DefaultSlaveId,
                        torque = DefaultValues.DefaultTorque,
                        velocity = DefaultValues.DefaultVelocity,
                        time = DefaultValues.DefaultTime,
                        angle = DefaultValues.DefaultAngle
                    };
                case FunctionType.ScrewParameters:
                    return new RequestData
                    {
                        request = FunctionCodes.ScrewParameters,
                        slave_id = DefaultSlaveId,
                        screw = 1,
                        torqueCompensation = 0.000,
                        torqueValidTime = 50,
                        torqueFilterTime = DefaultValues.DefaultTorqueFilterTime,
                        screwCount = DefaultValues.DefaultScrewCount,
                        velocityTarget = 0.0,
                        velocityMin = 0.0,
                        velocityMax = 0.0,
                        torqueTarget = 0.000,
                        torqueMin = 0.000,
                        torqueMax = 0.000,
                        angleTarget = 0.0,
                        angleMin = 0.0,
                        angleMax = 0.0,
                        stepCount = DefaultValues.DefaultStepCount,
                        stepValidStart = DefaultValues.DefaultStepValidStart,
                        steps = new[]
                        {
                            new Step { Index = 1, Velocity = 500.0, Torque = 0.015, Angle = 3600.0, Time = 2000, VelocityFrom = 0.0, VelocityTo = 0.0, Dir = 1, Es = 0, OkIf = new[] { 2, 8 } },
                            new Step { Index = 2, Velocity = 60.0, Torque = 0.049, Angle = 3600.0, Time = 2000, VelocityFrom = 0.0, VelocityTo = 0.0, Dir = 1, Es = 0, OkIf = new[] { 2 } }
                        }
                    };
                case FunctionType.ProductParameters:
                    return new RequestData
                    {
                        request = FunctionCodes.ProductParameters,
                        slave_id = DefaultSlaveId,
                        product_id = DefaultValues.DefaultProductId,
                        product_Name = DefaultValues.DefaultProductName,
                        screw = 1
                    };
                case FunctionType.ControlIdleRun:
                    return new RequestData
                    {
                        request = FunctionCodes.ControlIdleRun,
                        slave_id = DefaultSlaveId,
                        velocity = DefaultValues.DefaultVelocity,
                        time = DefaultValues.DefaultTime,
                        angle = DefaultValues.DefaultAngle
                    };
                case FunctionType.RemoveScrewAction:
                    return new RequestData
                    {
                        request = FunctionCodes.RemoveScrewAction,
                        slave_id = DefaultSlaveId,
                        torque = DefaultValues.DefaultTorque,
                        velocity = DefaultValues.DefaultVelocity,
                        time = DefaultValues.DefaultTime,
                        angle = DefaultValues.DefaultAngle
                    };
                case FunctionType.LockScrewAction:
                    return new RequestData
                    {
                        request = FunctionCodes.LockScrewAction,
                        slave_id = DefaultSlaveId,
                        product_id = DefaultValues.DefaultProductId
                    };
                case FunctionType.TorqueTest:
                    return new RequestData
                    {
                        request = FunctionCodes.TorqueTest,
                        slave_id = DefaultSlaveId,
                        currentPercent = DefaultValues.DefaultCurrentPercent,
                        torqueValidTime = DefaultValues.DefaultTorqueValidTime,
                        velocity = 100
                    };
                case FunctionType.Stop:
                    return new RequestData
                    {
                        request = FunctionCodes.Stop,
                        slave_id = DefaultSlaveId
                    };
                case FunctionType.StatusQuery:
                    return new RequestData
                    {
                        request = FunctionCodes.StatusQuery,
                        slave_id = DefaultSlaveId
                    };
                case FunctionType.ProductInfoQuery:
                    return new RequestData
                    {
                        request = FunctionCodes.ProductInfoQuery,
                        slave_id = DefaultSlaveId
                    };
                case FunctionType.TorqueCalibration:
                    return new RequestData
                    {
                        request = FunctionCodes.TorqueCalibration,
                        slave_id = DefaultSlaveId,
                        currentPercent = DefaultValues.DefaultCurrentPercent,
                        torque = 0.5
                    };
                case FunctionType.MotorSelfTest:
                    return new RequestData
                    {
                        request = FunctionCodes.MotorSelfTest,
                        slave_id = DefaultSlaveId,
                        ctrl = DefaultValues.DefaultCtrl
                    };
                case FunctionType.LockMode:
                    return new RequestData
                    {
                        request = FunctionCodes.LockMode,
                        slave_id = DefaultSlaveId,
                        product_id = DefaultValues.DefaultProductId
                    };
                case FunctionType.PowerControl:
                    return new RequestData
                    {
                        request = FunctionCodes.PowerControl,
                        slave_id = DefaultSlaveId,
                        powerEnable = DefaultValues.DefaultPowerEnable
                    };
                case FunctionType.TightenInfoControl:
                    return new RequestData
                    {
                        request = FunctionCodes.TightenInfoControl,
                        slave_id = DefaultSlaveId,
                        tightenClear = DefaultValues.DefaultTightenClear
                    };
                case FunctionType.WriteBarcode:
                    return new RequestData
                    {
                        request = FunctionCodes.WriteBarcode,
                        slave_id = DefaultSlaveId,
                        barCode = DefaultValues.DefaultBarCode
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(functionType), functionType, null);
            }
        }
    }
}