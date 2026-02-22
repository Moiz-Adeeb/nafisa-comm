using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PayRollConfiguration : IEntityTypeConfiguration<PayRoll>
{
    public void Configure(EntityTypeBuilder<PayRoll> builder)
    {
        builder.HasKey(p => p.Id);

        builder
            .HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId);

        builder
            .HasOne(p => p.CompanyStaff)
            .WithMany()
            .HasForeignKey(p => p.CompanyStaffId);

        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.LoanDeduction).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.NetAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Month).IsRequired();
        builder.Property(p => p.Year).IsRequired();
    }
}