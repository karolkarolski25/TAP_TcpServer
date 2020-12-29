using System.Threading.Tasks;
using Login.Enums;

namespace Login.Services
{
    public interface ILoginService
    {
        Task<bool> RegisterAccount(string login, string password);
        Task<UserLoginSettings> CheckData(string login, string password);
        Task<bool> ChangePassword(string login, string password);
    }
}
