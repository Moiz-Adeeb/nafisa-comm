using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Subscriptions.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Subscriptions.Commands;

public class CreateSubscriptionRequestModel : IRequest<CreateSubscriptionResponseModel>
{
    public string PlanName { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public decimal Price { get; set; }
    public int MaxUsersAllowed { get; set; }
    public int MaxBranches { get; set; }
    public string Features { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubscriptionRequestModelValidator
    : AbstractValidator<CreateSubscriptionRequestModel>
{
    public CreateSubscriptionRequestModelValidator()
    {
        RuleFor(p => p.PlanName).Required().Max(100);
        RuleFor(p => p.BillingCycle)
            .IsInEnum()
            .WithMessage("Billing cycle must be Monthly or Yearly");
        RuleFor(p => p.Price).Required();
        RuleFor(p => p.MaxUsersAllowed).Required();
        RuleFor(p => p.MaxBranches).Required();
        RuleFor(p => p.Features).Max(2000);
    }
}

public class CreateSubscriptionRequestHandler
    : IRequestHandler<CreateSubscriptionRequestModel, CreateSubscriptionResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public CreateSubscriptionRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<CreateSubscriptionResponseModel> Handle(
        CreateSubscriptionRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var planName = request.PlanName.Trim();
        var isExist = await _context.Subscriptions.AnyAsync(
            s => s.PlanName == planName && !s.IsDeleted,
            cancellationToken
        );

        if (isExist)
        {
            throw new AlreadyExistsException(
                $"Subscription plan with name '{planName}' already exists"
            );
        }

        var subscription = new Subscription
        {
            PlanName = planName,
            BillingCycle = request.BillingCycle,
            Price = request.Price,
            MaxUsersAllowed = request.MaxUsersAllowed,
            MaxBranches = request.MaxBranches,
            Features = request.Features,
            IsActive = request.IsActive,
        };

        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Subscription,
                Action = AuditLogType.Create,
                Description =
                    $"Subscription plan '{subscription.PlanName}' created with price {subscription.Price} CFA",
                DescriptionFr =
                    $"Plan d'abonnement '{subscription.PlanName}' créé avec un prix de {subscription.Price} CFA",
                EntityId = subscription.Id,
            }
        );

        return new CreateSubscriptionResponseModel { Data = new SubscriptionDto(subscription) };
    }
}

public class CreateSubscriptionResponseModel
{
    public SubscriptionDto Data { get; set; }
}
