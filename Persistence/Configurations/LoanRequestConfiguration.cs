using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class LoanRequestConfiguration : IEntityTypeConfiguration<LoanRequest>
{
    public void Configure(EntityTypeBuilder<LoanRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);

        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);

        builder.Property(e => e.InterestRate).IsRequired().HasPrecision(5, 2);

        builder.Property(e => e.TotalPayback).IsRequired().HasPrecision(18, 2);

        builder.Property(e => e.MonthlyPayment).IsRequired().HasPrecision(18, 2);

        builder.Property(e => e.Duration).IsRequired();

        builder.Property(e => e.Purpose).HasMaxLength(1000);

        builder.Property(e => e.DocumentPath).HasMaxLength(500);

        builder.Property(e => e.RejectionReason).HasMaxLength(500);

        builder.Property(e => e.Status).IsRequired();

        // Relationships
        builder
            .HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.CompanyStaff)
            .WithMany(p => p.LoanRequests)
            .HasForeignKey(e => e.CompanyStaffId);

        builder
            .HasOne(e => e.LoanOffer)
            .WithMany(o => o.LoanRequests)
            .HasForeignKey(e => e.LoanOfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.ApprovedBy)
            .WithMany()
            .HasForeignKey(e => e.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(e => e.RejectedBy)
            .WithMany()
            .HasForeignKey(e => e.RejectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.CompanyId);
        builder.HasIndex(e => e.LoanOfferId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedDate);
    }
}
