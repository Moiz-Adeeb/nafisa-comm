using System.Linq.Expressions;
using Application.Services.Subscriptions.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Subscriptions.Queries;

public class GetSubscriptionsDropDownRequestModel : IRequest<GetSubscriptionsDropDownResponseModel>
{
    public string Search { get; set; }
    public int? Limit { get; set; }
    public bool? IsActive { get; set; }
}

public class GetSubscriptionsDropDownRequestModelValidator
    : AbstractValidator<GetSubscriptionsDropDownRequestModel>
{
    public GetSubscriptionsDropDownRequestModelValidator() { }
}

public class GetSubscriptionsDropDownRequestHandler
    : IRequestHandler<GetSubscriptionsDropDownRequestModel, GetSubscriptionsDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetSubscriptionsDropDownRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetSubscriptionsDropDownResponseModel> Handle(
        GetSubscriptionsDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Subscription, bool>> query = p => true;

        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p => p.PlanName.ToLower().Contains(request.Search.ToLower()));
        }

        if (request.IsActive.HasValue)
        {
            query = query.AndAlso(p => p.IsActive == request.IsActive);
        }

        var queryable = _context.Subscriptions.GetAllReadOnly(query);

        if (request.Limit.HasValue)
        {
            queryable = queryable.Take(request.Limit.Value);
        }

        var list = await queryable
            .Select(SubscriptionSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetSubscriptionsDropDownResponseModel { Data = list };
    }
}

public class GetSubscriptionsDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
