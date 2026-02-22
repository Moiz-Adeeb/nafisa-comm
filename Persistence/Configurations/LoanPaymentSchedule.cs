using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class LoanPaymentScheduleConfiguration : IEntityTypeConfiguration<LoanPaymentSchedule>
{
    public void Configure(EntityTypeBuilder<LoanPaymentSchedule> builder)
    {
        builder
            .HasOne(p => p.CompanyStaff)
            .WithMany(p => p.LoanPaymentSchedules)
            .HasForeignKey(p => p.CompanyStaffId);
        builder
            .HasOne(p => p.LoanRequest)
            .WithMany(p => p.LoanPaymentSchedules)
            .HasForeignKey(p => p.LoanRequestId);
    }
}
