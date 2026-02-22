using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class LoanOfferConfiguration : IEntityTypeConfiguration<LoanOffer>
{
    public void Configure(EntityTypeBuilder<LoanOffer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(100);

        builder.Property(e => e.InterestRate).IsRequired().HasPrecision(5, 2);

        builder.Property(e => e.LoanMin).IsRequired().HasPrecision(18, 2);

        builder.Property(e => e.LoanMax).IsRequired().HasPrecision(18, 2);

        builder.Property(e => e.Durations).HasMaxLength(50);

        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.Property(e => e.IsActive).HasDefaultValue(true);

        // Relationships
        builder
            .HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.IsActive);
    }
}
