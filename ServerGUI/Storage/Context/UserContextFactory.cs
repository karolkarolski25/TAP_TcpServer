using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Storage.Context
{
    public class UserContextFactory : IDesignTimeDbContextFactory<UserContext>
    {
        /// <summary>
        /// Creates database context
        /// </summary>
        /// <param name="args">Additional parameters</param>
        /// <returns>new database context</returns>
        UserContext IDesignTimeDbContextFactory<UserContext>.CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            return new UserContext(configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>(),
                new DbContextOptionsBuilder<UserContext>().Options);
        }
    }
}
