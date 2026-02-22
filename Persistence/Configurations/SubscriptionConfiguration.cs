using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Features).HasMaxLength(2000);
        builder.Property(p => p.BillingCycle).IsRequired();
        builder.Property(p => p.MaxUsersAllowed).IsRequired();
        builder.Property(p => p.MaxBranches).HasDefaultValue(1);
    }
}
