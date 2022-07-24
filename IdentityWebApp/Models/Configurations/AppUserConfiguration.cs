using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityWebApp.Models.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.City)
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(x => x.BirthDate)
                .IsRequired(false);

            builder.Property(x => x.Picture)
                .IsRequired(false);
        }
    }
}
