using Application.Interfaces;
using Domain.Constant;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services;

public class AlertService : IAlertService
{
    private readonly IHubContext<NotificationHub> _hub;

    public AlertService(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task<bool> SendNotificationToAdmin(
        string title,
        string message,
        string type = "alert"
    )
    {
        await _hub
            .Clients.Group(RoleNames.Administrator)
            .SendCoreAsync("onNotification", [title, message, type]);
        return true;
    }

    public async Task<bool> SendNotificationToUser(
        string userId,
        string title,
        string message,
        string type = "alert"
    )
    {
        await _hub.Clients.Group(userId).SendCoreAsync("onNotification", [title, message, type]);
        return true;
    }

    public async Task<bool> SendNotificationToRole(
        string role,
        string title,
        string message,
        string type = "alert"
    )
    {
        await _hub.Clients.Group(role).SendCoreAsync("onNotification", [title, message, type]);
        return true;
    }
}
