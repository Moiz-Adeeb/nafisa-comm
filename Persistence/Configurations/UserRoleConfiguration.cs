using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId });

            builder.HasOne(p => p.Role).WithMany(p => p.UserRoles).HasForeignKey(p => p.RoleId);
            builder.HasOne(p => p.User).WithMany(p => p.UserRoles).HasForeignKey(p => p.UserId);
        }
    }
}
