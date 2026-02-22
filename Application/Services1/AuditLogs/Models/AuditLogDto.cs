using System.Linq.Expressions;
using Domain.Entities;
using Domain.Enums;
using Domain.Enums;

namespace Application.Services.AuditLogs.Models;

public class AuditLogDto
{
    public string User { get; set; }
    public string UserId { get; set; }
    public AuditLogFeatureType Feature { get; set; } // e.g., "Order", "Product", "Invoice"
    public AuditLogType Action { get; set; } // "Create", "Update", "Delete"
    public string Description { get; set; } // e.g., "Order #1023 created with 3 items"
    public string EntityId { get; set; } // Optional: ID of the object affected
    public DateTimeOffset CreatedDate { get; set; }

    public AuditLogDto() { }

    public AuditLogDto(AuditLog log)
    {
        User = log.User?.FullName;
        UserId = log.User?.Id;
        Feature = log.Feature;
        Description = log.Description;
        EntityId = log.EntityId;
        CreatedDate = log.CreatedDate;
        Action = log.Action;
    }
}

public class AuditLogSelector
{
    public static Expression<Func<AuditLog, AuditLogDto>> GetSelector(Lang? lang)
    {
        return p => new AuditLogDto
        {
            Feature = p.Feature,
            EntityId = p.EntityId,
            UserId = p.UserId,
            CreatedDate = p.CreatedDate,
            Action = p.Action,
            Description = lang == Lang.Fr ? p.DescriptionFr : p.Description,
            User = p.User.FullName,
        };
    }
}
