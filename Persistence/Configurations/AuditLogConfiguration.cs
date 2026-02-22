using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasIndex(p => p.EntityId);
        builder.HasIndex(p => p.ParentId);
        builder.HasOne(p => p.User).WithMany(p => p.AuditLogs).HasForeignKey(p => p.UserId);
    }
}
