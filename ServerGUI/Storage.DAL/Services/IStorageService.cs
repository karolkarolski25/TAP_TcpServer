using Storage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage.DAL
{
    public interface IStorageService
    {
        Task MigrateAsync();
        Task SaveChangesAsync();
        Task<List<User>> GetUserDataAsync();
        void AddUserDataAsync(User userData);
        void RemoveUserDataAsync(User userData);
        void UpdateData(User userData);
        void EditData();

        User Users { get; set; }
    }
}
