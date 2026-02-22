using Application.Interfaces;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AppNotifications.Commands;

public class SendAppNotificationToAllUsersRequestModel
    : IRequest<SendAppNotificationToAllUsersResponseModel>
{
    public string Title { get; set; }
    public string Body { get; set; }
    public NotificationType Type { get; set; }
    public string Data { get; set; }
    public string Role { get; set; }
}

public class SendAppNotificationToAllUsersRequestModelValidator
    : AbstractValidator<SendAppNotificationToAllUsersRequestModel>
{
    public SendAppNotificationToAllUsersRequestModelValidator() { }
}

public class SendAppNotificationToAllUsersRequestHandler
    : IRequestHandler<
        SendAppNotificationToAllUsersRequestModel,
        SendAppNotificationToAllUsersResponseModel
    >
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IAlertService _alertService;

    public SendAppNotificationToAllUsersRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IAlertService alertService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _alertService = alertService;
    }

    public async Task<SendAppNotificationToAllUsersResponseModel> Handle(
        SendAppNotificationToAllUsersRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var users = await _context
            .Users.GetManyReadOnly(p => p.UserRoles.Any(x => x.Role.Name == request.Role))
            .Select(p => new
            {
                p.Id,
                IsAllowNotification = p
                    .UserSettings.FirstOrDefault(x =>
                        x.Key == SettingKeyConstant.AllowNotificationSetting
                    )
                    .Value,
            })
            .ToListAsync(cancellationToken: cancellationToken);
        var notifications = users
            .Where(p => p.IsAllowNotification == true.ToString())
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
        foreach (var user in users.Where(user => user.IsAllowNotification == true.ToString()))
        {
            await _alertService.SendNotificationToUser(user.Id, request.Title, request.Body);
        }
        return new SendAppNotificationToAllUsersResponseModel();
    }
}

public class SendAppNotificationToAllUsersResponseModel
{
    public SendAppNotificationToAllUsersResponseModel() { }
}
