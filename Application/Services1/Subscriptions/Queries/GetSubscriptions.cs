using System.Linq.Expressions;
using Application.Extensions;
using Application.Services.Subscriptions.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Subscriptions.Queries;

public class GetSubscriptionsRequestModel : GetPagedRequest<GetSubscriptionsResponseModel>
{
    public bool? IsActive { get; set; }
    public BillingCycle? BillingCycle { get; set; }
}

public class GetSubscriptionsRequestModelValidator
    : PageRequestValidator<GetSubscriptionsRequestModel> { }

public class GetSubscriptionsRequestHandler
    : IRequestHandler<GetSubscriptionsRequestModel, GetSubscriptionsResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetSubscriptionsRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetSubscriptionsResponseModel> Handle(
        GetSubscriptionsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Subscription, bool>> query = p => true;

        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.PlanName.ToLower().Contains(request.Search.ToLower())
                || p.Features.ToLower().Contains(request.Search.ToLower())
            );
        }

        if (request.IsActive.HasValue)
        {
            query = query.AndAlso(p => p.IsActive == request.IsActive);
        }

        if (request.BillingCycle.HasValue)
        {
            query = query.AndAlso(p => p.BillingCycle == request.BillingCycle);
        }

        var list = await _context
            .Subscriptions.GetManyReadOnly(query, request)
            .Select(SubscriptionSelector.Selector)
            .ToListAsync(cancellationToken);

        var count = await _context.Subscriptions.ActiveCount(query, cancellationToken);

        return new GetSubscriptionsResponseModel { Data = list, Count = count };
    }
}

public class GetSubscriptionsResponseModel
{
    public List<SubscriptionDto> Data { get; set; }
    public int Count { get; set; }
}
