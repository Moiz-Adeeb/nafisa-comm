using Application.Extensions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AppNotifications.Commands;

public class SendAppNotificationRequestModel : IRequest<SendAppNotificationResponseModel>
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string Data { get; set; }
    public NotificationType Type { get; set; }
}

public class SendAppNotificationRequestModelValidator
    : AbstractValidator<SendAppNotificationRequestModel>
{
    public SendAppNotificationRequestModelValidator()
    {
        RuleFor(x => x.Id).Required();
    }
}

public class SendAppNotificationRequestHandler
    : IRequestHandler<SendAppNotificationRequestModel, SendAppNotificationResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private IBackgroundTaskQueueService _backgroundTaskQueueService;
    private readonly IAlertService _alertService;

    public SendAppNotificationRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService backgroundTaskQueueService,
        IAlertService alertService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _backgroundTaskQueueService = backgroundTaskQueueService;
        _alertService = alertService;
    }

    public async Task<SendAppNotificationResponseModel> Handle(
        SendAppNotificationRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var users = new List<User>() { new User() { Id = request.Id } };
        var notifications = users
            .Select(p => new AppNotification()
            {
                UserId = p.Id,
                Message = request.Body,
                Data = request.Data,
                Type = request.Type,
            })
            .ToList();
        await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await _alertService.SendNotificationToUser(request.Id, request.Title, request.Body);
        return new SendAppNotificationResponseModel();
    }
}

public class SendAppNotificationResponseModel { }
