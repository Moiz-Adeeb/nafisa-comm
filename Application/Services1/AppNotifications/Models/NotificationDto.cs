using System.Linq.Expressions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.AppNotifications.Models;

public class NotificationDto
{
    public string Id { get; set; }
    public string Message { get; set; }
    public string Data { get; set; }

    /// <summary>
    /// <see cref="Domain.Enums.NotificationType"/>
    /// </summary>
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public NotificationDto() { }

    public NotificationDto(AppNotification notification)
    {
        Id = notification.Id;
        Message = notification.Message;
        Data = notification.Data;
        Type = notification.Type;
        IsRead = notification.IsRead;
        CreatedDate = notification.CreatedDate;
    }
}

public class NotificationSelector
{
    public static readonly Expression<Func<AppNotification, NotificationDto>> Selector =
        p => new NotificationDto()
        {
            Data = p.Data,
            Id = p.Id,
            Message = p.Message,
            Type = p.Type,
            CreatedDate = p.CreatedDate,
            IsRead = p.IsRead,
        };
}
