using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Companies.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Companies.Commands;

public class AssignSubscriptionRequestModel : IRequest<AssignSubscriptionResponseModel>
{
    public string CompanyId { get; set; }
    public string SubscriptionId { get; set; }
    public int DurationMonths { get; set; } = 12; // Default 1 year
}

public class AssignSubscriptionRequestModelValidator
    : AbstractValidator<AssignSubscriptionRequestModel>
{
    public AssignSubscriptionRequestModelValidator()
    {
        RuleFor(p => p.CompanyId).Required();
        RuleFor(p => p.SubscriptionId).Required();
        RuleFor(p => p.DurationMonths)
            .GreaterThan(0)
            .WithMessage("Duration must be at least 1 month");
    }
}

public class AssignSubscriptionRequestHandler
    : IRequestHandler<AssignSubscriptionRequestModel, AssignSubscriptionResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public AssignSubscriptionRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<AssignSubscriptionResponseModel> Handle(
        AssignSubscriptionRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var company = await _context.Companies.FirstOrDefaultAsync(
            c => c.Id == request.CompanyId && !c.IsDeleted,
            cancellationToken
        );

        if (company == null)
        {
            throw new NotFoundException("Company not found");
        }

        if (company.Status != RequestStatus.Approved)
        {
            throw new BadRequestException("Company must be approved before assigning subscription");
        }

        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(
            s => s.Id == request.SubscriptionId && !s.IsDeleted,
            cancellationToken
        );

        if (subscription == null)
        {
            throw new NotFoundException("Subscription plan not found");
        }

        if (!subscription.IsActive)
        {
            throw new BadRequestException("Selected subscription plan is not active");
        }

        // Assign subscription
        company.SubscriptionId = request.SubscriptionId;
        company.SubscriptionStartDate = DateTimeOffset.UtcNow;
        company.SubscriptionEndDate = DateTimeOffset.UtcNow.AddMonths(request.DurationMonths);
        company.SubscriptionActive = true;

        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Subscription '{subscription.PlanName}' assigned to company '{company.CompanyName ?? company.Email}' for {request.DurationMonths} months",
                DescriptionFr =
                    $"Abonnement '{subscription.PlanName}' attribué à l'entreprise '{company.CompanyName ?? company.Email}' pour {request.DurationMonths} mois",
                EntityId = company.Id,
            }
        );

        // TODO: Send subscription activation email to company

        return new AssignSubscriptionResponseModel
        {
            Data = new CompanyDto(company),
            Message =
                $"Subscription assigned successfully. Valid until {company.SubscriptionEndDate:yyyy-MM-dd}",
        };
    }
}

public class AssignSubscriptionResponseModel
{
    public CompanyDto Data { get; set; }
    public string Message { get; set; }
}
