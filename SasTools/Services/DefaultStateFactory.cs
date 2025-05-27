using SasTools.Domain;
using SasTools.Interface;
using SasTools.States;
using System;
using System.Collections.Generic;

namespace SasTools.Services
{
    // 默认状态工厂实现
    public class DefaultStateFactory : IStateFactory
    {
        private readonly Dictionary<TestState, IState> _stateCache = new Dictionary<TestState, IState>();

        // 创建状态对象
        public IState CreateState(TestState stateEnum)
        {
            // 检查缓存
            if (_stateCache.TryGetValue(stateEnum, out var state))
            {
                return state;
            }

            // 创建新状态
            switch (stateEnum)
            {
                case TestState.Idle:
                    state = new IdleState();
                    break;
                case TestState.Initializing:
                    state = new InitializingState();
                    break;
                case TestState.ForwardDelay:
                    state = new ForwardDelayState();
                    break;
                case TestState.Forward:
                    state = new ForwardState();
                    break;
                case TestState.ForwardWaiting:
                    state = new ForwardWaitingState();
                    break;
                case TestState.RotationInterval:
                    state = new RotationIntervalState();
                    break;
                case TestState.ReverseDelay:
                    state = new ReverseDelayState();
                    break;
                case TestState.Reverse:
                    state = new ReverseState();
                    break;
                case TestState.ReverseWaiting:
                    state = new ReverseWaitingState();
                    break;
                case TestState.StartupInterval:
                    state = new StartupIntervalState();
                    break;
                case TestState.Stopping:
                    state = new StoppingState();
                    break;
                case TestState.Error:
                    state = new ErrorState();
                    break;
                default:
                    throw new ArgumentException($"未知的状态枚举值: {stateEnum}");
            }

            // 缓存状态对象
            _stateCache[stateEnum] = state;
            return state;
        }
    }
}


