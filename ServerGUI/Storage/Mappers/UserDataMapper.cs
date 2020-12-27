using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Storage.Models;

namespace Storage.Mappers
{
    public class UserDataMapper : IEntityTypeConfiguration<UserData>
    {
        /// <summary>
        /// Configures database
        /// </summary>
        /// <param name="builder">Database scheme</param>
        public void Configure(EntityTypeBuilder<UserData> builder)
        {
            builder.HasIndex(p => p.Usernane)
                .IsUnique();

            builder.Property(p => p.Usernane)
                .IsRequired();

            builder.Property(p => p.Password)
                .IsRequired();
        }
    }
}
