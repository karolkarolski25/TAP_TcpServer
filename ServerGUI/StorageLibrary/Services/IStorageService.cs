using StorageLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StorageLibrary.Services
{
    public interface IStorageService
    {
        Task MigrateAsync();
        Task SaveChangesAsync();
        Task<List<UserData>> GetUserDataAsync();
        void AddUserDataAsync(UserData userData);
        void RemoveUserDataAsynv(UserData userData);
        void UpdateData(UserData userData);
        void EditData();

        UserData UserData { get; set; }
    }
}
