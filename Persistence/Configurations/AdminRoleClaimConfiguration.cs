using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class AdminRoleClaimConfiguration : IEntityTypeConfiguration<AdminRoleClaim>
{
    public void Configure(EntityTypeBuilder<AdminRoleClaim> builder)
    {
        builder.HasIndex(p => new { p.AdminRoleId, p.ClaimValue }).IsUnique();
        builder.Property(p => p.ClaimType).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ClaimValue).IsRequired().HasMaxLength(255);
    }
}
