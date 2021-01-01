using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Storage.Mappers;
using Storage.Models;

namespace Storage.Context
{
    public class UserContext : DbContext, IUserContext
    {
        public DbSet<User> Users { get; set; }

        private readonly DatabaseConfiguration _databaseConfiguration;

        public UserContext(DatabaseConfiguration databaseConfiguration,
            DbContextOptions<UserContext> dbContextOptions) : base(dbContextOptions)
        {
            _databaseConfiguration = databaseConfiguration;
        }

        /// <summary>
        /// Configure database
        /// </summary>
        /// <param name="dbContextOptionsBuilder">database options</param>
        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            if (!dbContextOptionsBuilder.IsConfigured)
            {
                dbContextOptionsBuilder.UseSqlServer(_databaseConfiguration.ConnectionString);
                base.OnConfiguring(dbContextOptionsBuilder);
            }
        }

        /// <summary>
        /// Apply database configuration
        /// </summary>
        /// <param name="modelBuilder">Database builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMapper());
        }
    }
}
