using Application.Exceptions;
using Application.Extensions;
using Application.Services.Subscriptions.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Subscriptions.Queries;

public class GetSubscriptionByIdRequestModel : IRequest<GetSubscriptionByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetSubscriptionByIdRequestModelValidator
    : AbstractValidator<GetSubscriptionByIdRequestModel>
{
    public GetSubscriptionByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetSubscriptionByIdRequestHandler
    : IRequestHandler<GetSubscriptionByIdRequestModel, GetSubscriptionByIdResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetSubscriptionByIdRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetSubscriptionByIdResponseModel> Handle(
        GetSubscriptionByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Subscriptions.GetByWithSelectAsync(
            p => p.Id == request.Id,
            SubscriptionSelector.Selector,
            cancellationToken: cancellationToken
        );

        if (data == null)
        {
            throw new NotFoundException(nameof(Subscription));
        }

        return new GetSubscriptionByIdResponseModel { Data = data };
    }
}

public class GetSubscriptionByIdResponseModel
{
    public SubscriptionDto Data { get; set; }
}
