using SasTools.Domain;
using SasTools.Interface;

namespace SasTools.Interface
{
    // 状态工厂接口 - 负责创建状态对象
    public interface IStateFactory
    {
        // 创建状态对象
        IState CreateState(TestState stateEnum);
    }
}

