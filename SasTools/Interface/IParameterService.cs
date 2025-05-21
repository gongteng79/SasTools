using SasTools.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    //参数服务接口，定义参数保存和加载相关操作
    public interface IParameterService
    {
        //保存测试参数
        Task<bool> SaveParameterAsync(TestParameters parameters);

        //加载测试参数
        Task<TestParameters> LoadParameterAsync();

        //验证参数是否有效
        bool ValidateParameters(TestParameters parameters, out string errorMessage);
    }
}
