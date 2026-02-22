using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder
                .HasMany(p => p.UserRoles)
                .WithOne(p => p.Role)
                .HasForeignKey(p => p.RoleId)
                .IsRequired();
        }
    }
}
