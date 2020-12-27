using System.Threading.Tasks;

namespace Server.Services
{
    public interface IServerService
    {
        Task StartServer(string ipAddress, int port);    
    }
}
