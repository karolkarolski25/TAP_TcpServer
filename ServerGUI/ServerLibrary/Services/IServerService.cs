using System.Threading.Tasks;

namespace ServerLibrary.Services
{
    public interface IServerService
    {
        Task StartServer(string ipAddress, int port);    
    }
}
