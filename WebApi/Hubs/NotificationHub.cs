using System.Collections.Concurrent;
using Application.Interfaces;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebApi.Extension;

namespace WebApi.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> Connections = new();

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var roleNames = RoleNames.AllRoles;
        foreach (var roleName in roleNames)
        {
            if (Context.User != null && Context.User.IsInRole(roleName))
            {
                Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    roleName,
                    Context.ConnectionAborted
                );
            }
        }
        Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            Context.User.GetUserId(),
            Context.ConnectionAborted
        );
        Connections.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync()
    {
        foreach (var allRole in RoleNames.AllRoles)
        {
            if (Context.User != null && Context.User.IsInRole(allRole))
            {
                Groups.AddToGroupAsync(Context.ConnectionId, allRole, Context.ConnectionAborted);
            }
        }

        Groups.AddToGroupAsync(
            Context.ConnectionId,
            Context.User.GetUserId(),
            Context.ConnectionAborted
        );
        Connections.TryAdd(Context.ConnectionId, Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public string GetConnectionId() => Context.ConnectionId;

    public static List<string> GetConnectionIds()
    {
        return Connections.Keys.ToList();
    }
}
