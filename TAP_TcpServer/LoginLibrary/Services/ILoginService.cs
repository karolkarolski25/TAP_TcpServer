using System.Net.Sockets;
using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        bool RegisterAccount(string data);
        bool CheckData(string data);
        Task<string> GetLoginString(NetworkStream stream, byte[] buffer);
        Task<string> GetPasswordString(NetworkStream stream, byte[] buffer, string data);
    }
}
