using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StorageLibrary.Models;
using System.Threading;
using System.Threading.Tasks;

namespace StorageLibrary.Context
{
    public interface IUserDataContext
    {
        DbSet<UserData> UserDatas { get; set; }
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
