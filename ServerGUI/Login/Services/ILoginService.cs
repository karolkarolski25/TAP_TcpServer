using System.Threading.Tasks;

namespace Login.Services
{
    public interface ILoginService
    {
        Task<bool> RegisterAccount(string login, string password);
        Task<bool> CheckData(string login, string password);
        Task<bool> ChangePassword(string login, string password);
    }
}
