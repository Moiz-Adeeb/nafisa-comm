using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AppNotifications.Commands;

public class ReadAllUnReadNotificationRequestModel
    : IRequest<ReadAllUnReadNotificationResponseModel> { }

public class ReadAllUnReadNotificationRequestModelValidator
    : AbstractValidator<ReadAllUnReadNotificationRequestModel> { }

public class ReadAllUnReadNotificationRequestHandler
    : IRequestHandler<ReadAllUnReadNotificationRequestModel, ReadAllUnReadNotificationResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public ReadAllUnReadNotificationRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<ReadAllUnReadNotificationResponseModel> Handle(
        ReadAllUnReadNotificationRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var notifications = await _context
            .Notifications.GetAll(p => p.IsRead == false && p.UserId == _sessionService.GetUserId())
            .ToListAsync(cancellationToken: cancellationToken);
        if (notifications.Any())
        {
            foreach (var appNotification in notifications)
            {
                appNotification.IsRead = true;
                appNotification.ReadAt = DateTimeOffset.UtcNow;
            }
            _context.UpdateRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return new ReadAllUnReadNotificationResponseModel() { };
    }
}

public class ReadAllUnReadNotificationResponseModel { }
