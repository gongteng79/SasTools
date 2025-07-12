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
        private int _forwardDelay;
        private int _reverseDelay;
        private int _timeout;
        private int _maxCycles = 0;
        private int _maxFailures = 0;
        private int _reverseVelocity = 500;  // 默认反转转速
        private int _reverseTime = 1000;     // 默认反转时间

        //正转启动延时(毫秒)
        public int ForwardDelay
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
        //反转启动延时(毫秒)
        public int ReverseDelay
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
        //超时设置(毫秒)
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
        public int MaxCycles
        {
            get { return _maxCycles; }
            set
            {
                if (_maxCycles != value)
                {
                    _maxCycles = value;
                    OnPropertyChanged(nameof(MaxCycles));
                }
            }
        }

        public int MaxFailures
        {
            get { return _maxFailures; }
            set
            {
                if (_maxFailures != value)
                {
                    _maxFailures = value;
                    OnPropertyChanged(nameof(MaxFailures));
                }
            }
        }

        //反转速度
        public int ReverseVelocity
        {
            get => _reverseVelocity;
            set
            {
                if (_reverseVelocity != value)
                {
                    _reverseVelocity = value;
                    OnPropertyChanged(nameof(ReverseVelocity));
                }
            }
        }

        //反转时间(毫秒)
        public int ReverseTime
        {
            get => _reverseTime;
            set
            {
                if (_reverseTime != value)
                {
                    _reverseTime = value;
                    OnPropertyChanged(nameof(ReverseTime));
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
