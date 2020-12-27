using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace StorageLibrary.Context
{
    public class UserDataContextFactory : IDesignTimeDbContextFactory<UserDataContext>
    {
        /// <summary>
        /// Creates database context
        /// </summary>
        /// <param name="args">Additional parameters</param>
        /// <returns>new database context</returns>
        UserDataContext IDesignTimeDbContextFactory<UserDataContext>.CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            return new UserDataContext(configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>(),
                new DbContextOptionsBuilder<UserDataContext>().Options);
        }
    }
}
