using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .HasOne(t => t.Company)
            .WithMany()
            .HasForeignKey(t => t.CompanyId);

        builder
            .HasOne(t => t.CompanyStaff)
            .WithMany()
            .HasForeignKey(t => t.CompanyStaffId);

        builder
            .HasOne(t => t.PayRoll)
            .WithMany()
            .HasForeignKey(t => t.PayRollId)
            .IsRequired(false);

        builder
            .HasOne(t => t.LoanPaymentSchedule)
            .WithMany()
            .HasForeignKey(t => t.LoanPaymentScheduleId)
            .IsRequired(false);

        builder.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.TransactionDate).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.DescriptionFr).HasMaxLength(500);
        builder.Property(t => t.Reference).HasMaxLength(100);
    }
}