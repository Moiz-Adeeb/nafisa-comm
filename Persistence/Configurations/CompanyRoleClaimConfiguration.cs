using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class CompanyRoleClaimConfiguration : IEntityTypeConfiguration<CompanyRoleClaim>
{
    public void Configure(EntityTypeBuilder<CompanyRoleClaim> builder)
    {
        builder.HasIndex(p => new { p.CompanyRoleId, p.ClaimValue }).IsUnique();
        builder.Property(p => p.ClaimType).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ClaimValue).IsRequired().HasMaxLength(255);
    }
}
