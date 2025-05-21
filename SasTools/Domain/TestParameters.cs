using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SasTools.Domain
{
    //测试参数模型类，用于存储循环测试的各项参数
    //INotifyPropertyChanged是一个接口，允许对象在属性值更改时通知订阅者（通常是 UI）
    public class TestParameters : INotifyPropertyChanged
    {
        private double _forwardDelay;
        private double _reverseDelay;
        private double _rotationInterval;
        private double _startupInterval;
        private int _timeout;

        //正转启动延时(秒)
        public double ForwardDelay
        {
            get => _forwardDelay;
            set
            {
                if (_forwardDelay != value)
                {
                    _forwardDelay = value;
                    OnPropertyChanged(nameof(ForwardDelay));
                }
            }
        }
        //反转启动延时(秒)
        public double ReverseDelay
        {
            get => _reverseDelay;
            set
            {
                if (_reverseDelay != value)
                {
                    _reverseDelay = value;
                    OnPropertyChanged(nameof(ReverseDelay));
                }
            }
        }
        //正反转切换间隔(秒)
        public double RotationInterval
        {
            get => _rotationInterval;
            set
            {
                if (_rotationInterval != value)
                {
                    _rotationInterval = value;
                    OnPropertyChanged(nameof(RotationInterval));
                }
            }
        }
        //循环启动间隔(秒)
        public double StartupInterval
        {
            get => _startupInterval;
            set
            {
                if (_startupInterval != value)
                {
                    _startupInterval = value;
                    OnPropertyChanged(nameof(StartupInterval));
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
                    OnPropertyChanged(nameof(Timeout));
                }
            }
        }

        //声明INotifyPropertyChanged接口事件
        //PropertyChanged 事件：当属性值发生变化时触发
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

    }
}
