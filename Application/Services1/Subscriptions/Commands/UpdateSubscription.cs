using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Subscriptions.Models;
using Domain.Enums;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Subscriptions.Commands;

public class UpdateSubscriptionRequestModel : IRequest<UpdateSubscriptionResponseModel>
{
    public string Id { get; set; }
    public string PlanName { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public decimal Price { get; set; }
    public int MaxUsersAllowed { get; set; }
    public int MaxBranches { get; set; }
    public string Features { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSubscriptionRequestModelValidator
    : AbstractValidator<UpdateSubscriptionRequestModel>
{
    public UpdateSubscriptionRequestModelValidator()
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

public class UpdateSubscriptionRequestHandler
    : IRequestHandler<UpdateSubscriptionRequestModel, UpdateSubscriptionResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public UpdateSubscriptionRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<UpdateSubscriptionResponseModel> Handle(
        UpdateSubscriptionRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(
            s => s.Id == request.Id && !s.IsDeleted,
            cancellationToken
        );

        if (subscription == null)
        {
            throw new BadRequestException("Subscription plan not found");
        }

        var planName = request.PlanName.Trim();
        var isExist = await _context.Subscriptions.AnyAsync(
            s => s.PlanName == planName && s.Id != request.Id && !s.IsDeleted,
            cancellationToken
        );

        if (isExist)
        {
            throw new AlreadyExistsException(
                $"Subscription plan with name '{planName}' already exists"
            );
        }

        subscription.PlanName = planName;
        subscription.BillingCycle = request.BillingCycle;
        subscription.Price = request.Price;
        subscription.MaxUsersAllowed = request.MaxUsersAllowed;
        subscription.MaxBranches = request.MaxBranches;
        subscription.Features = request.Features;
        subscription.IsActive = request.IsActive;

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Subscription,
                Action = AuditLogType.Update,
                Description = $"Subscription plan '{subscription.PlanName}' updated",
                DescriptionFr = $"Plan d'abonnement '{subscription.PlanName}' mis à jour",
                EntityId = subscription.Id,
            }
        );

        return new UpdateSubscriptionResponseModel { Data = new SubscriptionDto(subscription) };
    }
}

public class UpdateSubscriptionResponseModel
{
    public SubscriptionDto Data { get; set; }
}
