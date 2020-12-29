using Storage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.DAL
{
    public interface IStorageService
    {
        Task MigrateAsync();
        Task SaveChangesAsync();
        Task<List<UserData>> GetUserDataAsync();
        void AddUserDataAsync(UserData userData);
        void RemoveUserDataAsync(UserData userData);
        void UpdateData(UserData userData);
        void EditData();

        UserData UserDatas { get; set; }
    }
}
