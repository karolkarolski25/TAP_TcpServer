using StorageLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StorageLibrary.Mappers
{
    public class UserDataMapper : IEntityTypeConfiguration<UserData>
    {
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
