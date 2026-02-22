using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Subscriptions.Commands;

public class DeleteSubscriptionRequestModel : IRequest<DeleteSubscriptionResponseModel>
{
    public string Id { get; set; }
}

public class DeleteSubscriptionRequestModelValidator
    : AbstractValidator<DeleteSubscriptionRequestModel>
{
    public DeleteSubscriptionRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteSubscriptionRequestHandler
    : IRequestHandler<DeleteSubscriptionRequestModel, DeleteSubscriptionResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteSubscriptionRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteSubscriptionResponseModel> Handle(
        DeleteSubscriptionRequestModel request,
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

        // Soft delete
        subscription.IsDeleted = true;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Subscription,
                Action = AuditLogType.Delete,
                Description = $"Subscription plan '{subscription.PlanName}' deleted",
                DescriptionFr = $"Plan d'abonnement '{subscription.PlanName}' supprimé",
                EntityId = subscription.Id,
            }
        );

        return new DeleteSubscriptionResponseModel { Success = true };
    }
}

public class DeleteSubscriptionResponseModel
{
    public bool Success { get; set; }
}
