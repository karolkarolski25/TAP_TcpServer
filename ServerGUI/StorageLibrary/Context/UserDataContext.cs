using Microsoft.EntityFrameworkCore;
using StorageLibrary.Mappers;
using StorageLibrary.Models;

namespace StorageLibrary.Context
{
    public class UserDataContext : DbContext, IUserDataContext
    {
        public DbSet<UserData> UserDatas { get; set; }

        private readonly DatabaseConfiguration _databaseConfiguration;

        public UserDataContext(DatabaseConfiguration databaseConfiguration,
            DbContextOptions<UserDataContext> dbContextOptions) : base(dbContextOptions)
        {
            _databaseConfiguration = databaseConfiguration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.UseSqlServer(_databaseConfiguration.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserDataMapper());
        }
    }
}
