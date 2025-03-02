using SasTools.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasTools.Interface
{
    public interface ISasTest
    {
        bool ConnectServer();

        bool DisconnectServer();

        string ReadData(RequestData data);
    }
}
