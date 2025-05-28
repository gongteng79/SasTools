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
        public static RequestData CreateRequestCommand(FunctionType functionType, RequestParameter options)
        {
            var baseData = new RequestData
            {
                request = options.Request,
                slave_id = options.SlaveId,
                sequence = options.Sequence
            };

            switch (functionType)
            {
                case FunctionType.Subcribe:
                    baseData.keep_alive = options.KeepAlive ?? 1;
                    baseData.cache_clear = options.CacheClear ?? 0;
                    break;

                case FunctionType.InputScrewData:
                    baseData.screw = options.Screw ?? 0;
                    break;

                case FunctionType.IdleRunParameter:
                case FunctionType.ControlIdleRun:
                    baseData.velocity = options.Velocity ?? 500;
                    baseData.time = options.Time ?? 0;
                    baseData.angle = options.Angle ?? 0;
                    break;

                case FunctionType.RemoveScrewParameter:
                case FunctionType.RemoveScrewAction:
                    baseData.torque = options.Torque ?? 0.1;
                    baseData.velocity = options.Velocity ?? 500;
                    baseData.time = options.Time ?? 0;
                    baseData.angle = options.Angle ?? 0;
                    break;

                case FunctionType.ProductParameters:
                    baseData.product_id = options.ProductId ?? 1;
                    baseData.product_Name = options.ProductName ?? "Product1";
                    baseData.screw = options.Screw ?? 1;
                    break;

                case FunctionType.LockScrewAction:
                case FunctionType.LockMode:
                    baseData.product_id = options.ProductId ?? 1;
                    break;

                case FunctionType.TorqueTest:
                    baseData.currentPercent = options.CurrentPercent ?? 20;
                    baseData.torqueValidTime = 100;
                    baseData.velocity = options.Velocity ?? 100;
                    break;

                case FunctionType.TorqueCalibration:
                    baseData.currentPercent = options.CurrentPercent ?? 20;
                    baseData.torque = options.Torque ?? 0.5;
                    break;

                case FunctionType.MotorSelfTest:
                    baseData.ctrl = options.Ctrl ?? 0;
                    break;

                case FunctionType.PowerControl:
                    baseData.powerEnable = options.PowerEnable ?? 1;
                    break;

                case FunctionType.TightenInfoControl:
                    baseData.tightenClear = options.TightenClear ?? 2;
                    break;

                case FunctionType.WriteBarcode:
                    baseData.barCode = options.BarCode ?? 0;
                    break;

                case FunctionType.ScrewParameters:
                    baseData = new RequestData
                    {
                        request = options.Request,
                        slave_id = options.SlaveId,
                        screw = options.Screw ?? 1,
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
                    break;

                    // FunctionType.StatusQuery, Stop, ProductInfoQuery 可不需要额外参数
            }

            return baseData;
        }

    }
}
