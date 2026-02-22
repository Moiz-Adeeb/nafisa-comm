using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasIndex(p => p.Email).IsUnique();
        builder.HasIndex(p => p.PhoneNumber);
        builder.HasIndex(p => p.Status);

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(150);
        builder.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.CompanyName).HasMaxLength(200);
        builder.Property(p => p.BusinessRegistrationCertificate).HasMaxLength(500);
        builder.Property(p => p.VatCertificate).HasMaxLength(500);
        builder.Property(p => p.AuthorizationIdProof).HasMaxLength(500);
        builder.Property(p => p.AdminNotes).HasMaxLength(1000);

        // Navigation Properties
        builder
            .HasOne(p => p.ApprovedByUser)
            .WithMany()
            .HasForeignKey(p => p.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.RejectedByUser)
            .WithMany()
            .HasForeignKey(p => p.RejectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.Subscription)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(p => p.CompanyAdminUser)
            .WithMany()
            .HasForeignKey(p => p.CompanyAdminUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
