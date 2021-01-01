using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Storage.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Storage.Context
{
    public interface IUserContext
    {
        DbSet<User> Users { get; set; }
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
