using Application.Extensions;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AppNotifications.Commands;

public class ReadNotificationRequestModel : IRequest<ReadNotificationResponseModel>
{
    public string Id { get; set; }
}

public class ReadNotificationRequestModelValidator : AbstractValidator<ReadNotificationRequestModel>
{
    public ReadNotificationRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class ReadNotificationRequestHandler
    : IRequestHandler<ReadNotificationRequestModel, ReadNotificationResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public ReadNotificationRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<ReadNotificationResponseModel> Handle(
        ReadNotificationRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var notification = await _context.Notifications.GetByAsync(
            p => p.Id == request.Id && p.UserId == _sessionService.GetUserId(),
            cancellationToken: cancellationToken
        );
        if (notification is { IsRead: false })
        {
            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return new ReadNotificationResponseModel();
    }
}

public class ReadNotificationResponseModel { }
