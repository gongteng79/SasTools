using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasTools.Interface;

namespace SasTools.Domain
{
    public class FatigueParams : ParameterBase
    {
        private int _forwardDelay = 1000;
        private int _reverseDelay = 1000;
        private int _rotationInterval = 1000;
        private int _startupInterval = 1000;
        private int _timeout = 1000;

        //正转启动延时(秒)
        public int ForwardDelay
        {
            get => _forwardDelay;
            set
            {
                if (_forwardDelay != value)
                {
                    _forwardDelay = value;
                }
            }
        }
        //反转启动延时(秒)
        public int ReverseDelay
        {
            get => _reverseDelay;
            set
            {
                if (_reverseDelay != value)
                {
                    _reverseDelay = value;
                }
            }
        }
        //正反转切换间隔(秒)
        public int RotationInterval
        {
            get => _rotationInterval;
            set
            {
                if (_rotationInterval != value)
                {
                    _rotationInterval = value;
                }
            }
        }
        //循环启动间隔(秒)
        public int StartupInterval
        {
            get => _startupInterval;
            set
            {
                if (_startupInterval != value)
                {
                    _startupInterval = value;
                }
            }
        }
        //超时设置(秒)
        public int Timeout
        {
            get => _timeout;
            set
            {
                if (_timeout != value)
                {
                    _timeout = value;
                }
            }
        }

        public override Task<TestParameters> LoadParameterAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SaveParameterAsync(TestParameters parameters)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateParameters(TestParameters parameters, out string errorMessage)
        {
            throw new NotImplementedException();
        }  
    }
}
