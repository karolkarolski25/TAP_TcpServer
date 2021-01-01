using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Models;

namespace Storage.Mappers
{
    public class UserMapper : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// Configures database
        /// </summary>
        /// <param name="builder">Database scheme</param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(p => p.Login)
                .IsUnique();

            builder.Property(p => p.Login)
                .IsRequired();

            builder.Property(p => p.Password)
                .IsRequired();
        }
    }
}
