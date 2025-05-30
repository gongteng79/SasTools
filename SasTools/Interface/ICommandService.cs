using SasTools.Domain;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    // 命令服务接口 - 负责执行命令
    public interface ICommandService
    {
        // 执行命令
        Task<string> ExecuteCommand(CommandType commandType);
    }
}

