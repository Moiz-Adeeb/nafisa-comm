using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class CompanyStaffConfiguration : IEntityTypeConfiguration<CompanyStaff>
{
    public void Configure(EntityTypeBuilder<CompanyStaff> builder)
    {
        builder.HasKey(p => p.UserId);
        builder
            .HasOne(p => p.Company)
            .WithMany(p => p.CompanyStaffs)
            .HasForeignKey(p => p.CompanyId);
        builder
            .HasOne(p => p.User)
            .WithOne(p => p.CompanyStaff)
            .HasForeignKey<CompanyStaff>(p => p.UserId);

        builder
            .HasOne(p => p.CompanyRole)
            .WithMany(p => p.CompanyStaffs)
            .HasForeignKey(p => p.CompanyRoleId);
        builder.HasOne(p => p.Branch).WithMany(p => p.CompanyStaffs).HasForeignKey(p => p.BranchId);
    }
}
