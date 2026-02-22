using System.Linq.Expressions;
using Application.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AppNotifications.Queries;

public class GetUnReadNotificationCountRequestModel
    : IRequest<GetUnReadNotificationCountResponseModel> { }

public class GetUnReadNotificationCountRequestModelValidator
    : AbstractValidator<GetUnReadNotificationCountRequestModel> { }

public class GetUnReadNotificationCountRequestHandler
    : IRequestHandler<
        GetUnReadNotificationCountRequestModel,
        GetUnReadNotificationCountResponseModel
    >
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetUnReadNotificationCountRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetUnReadNotificationCountResponseModel> Handle(
        GetUnReadNotificationCountRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<AppNotification, bool>> query = p =>
            p.UserId == _sessionService.GetUserId() && p.IsRead == false;
        var count = await _context.Notifications.ActiveCount(query, cancellationToken);
        return new GetUnReadNotificationCountResponseModel() { Count = count };
    }
}

public class GetUnReadNotificationCountResponseModel
{
    public int Count { get; set; }
}
