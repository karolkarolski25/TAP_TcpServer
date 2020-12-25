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
            //dbContextOptionsBuilder.UseSqlServer("Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = UserDataDb; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserDataMapper());
        }
    }
}
