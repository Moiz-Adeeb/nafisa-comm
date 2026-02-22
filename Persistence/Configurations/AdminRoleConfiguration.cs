using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class AdminRoleConfiguration : IEntityTypeConfiguration<AdminRole>
{
    public void Configure(EntityTypeBuilder<AdminRole> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);

        builder
            .HasMany(p => p.AdminRoleClaims)
            .WithOne(p => p.AdminRole)
            .HasForeignKey(p => p.AdminRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(p => p.Users)
            .WithOne(p => p.AdminRole)
            .HasForeignKey(p => p.AdminRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
