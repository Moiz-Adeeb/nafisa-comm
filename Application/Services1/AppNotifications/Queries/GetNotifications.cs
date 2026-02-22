using System.Linq.Expressions;
using Application.Interfaces;
using Application.Services.AppNotifications.Models;
using Application.Shared;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AppNotifications.Queries;

public class GetNotificationsRequestModel : GetPagedRequest<GetNotificationsResponseModel>
{
    public bool? IsRead { get; set; }
}

public class GetNotificationsRequestModelValidator
    : PageRequestValidator<GetNotificationsRequestModel>
{
    public GetNotificationsRequestModelValidator() { }
}

public class GetNotificationsRequestHandler
    : IRequestHandler<GetNotificationsRequestModel, GetNotificationsResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetNotificationsRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetNotificationsResponseModel> Handle(
        GetNotificationsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        Expression<Func<AppNotification, bool>> query = p => p.UserId == userId;
        if (request.IsRead.HasValue)
        {
            var isRead = request.IsRead.Value;
            query = query.AndAlso(p => p.IsRead == isRead);
        }

        var list = await _context
            .Notifications.GetManyReadOnly(query, request)
            .Select(NotificationSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Notifications.ActiveCount(query, cancellationToken);
        return new GetNotificationsResponseModel() { Data = list, Count = count };
    }
}

public class GetNotificationsResponseModel
{
    public GetNotificationsResponseModel() { }

    public List<NotificationDto> Data { get; set; }
    public int Count { get; set; }
}
