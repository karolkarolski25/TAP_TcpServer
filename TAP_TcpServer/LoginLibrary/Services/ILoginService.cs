using System.Net.Sockets;
using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        bool RegisterAccount(string data);
        bool CheckData(string data);
    }
}
