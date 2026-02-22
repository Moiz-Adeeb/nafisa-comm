using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Companies.Queries;

public class CheckUserSubscriptionRequestModel : IRequest<CheckUserSubscriptionResponseModel> { }

public class CheckUserSubscriptionRequestHandler
    : IRequestHandler<CheckUserSubscriptionRequestModel, CheckUserSubscriptionResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CheckUserSubscriptionRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CheckUserSubscriptionResponseModel> Handle(
        CheckUserSubscriptionRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();

        // Find company associated with user
        var company = await _context
            .Companies.Include(c => c.Subscription)
            .FirstOrDefaultAsync(
                c => c.CompanyAdminUserId == userId && !c.IsDeleted,
                cancellationToken
            );

        if (company == null)
        {
            // User is not a company admin (might be system admin or employee)
            return new CheckUserSubscriptionResponseModel
            {
                HasSubscription = false,
                Message = "User is not associated with any company",
            };
        }

        if (string.IsNullOrEmpty(company.SubscriptionId))
        {
            return new CheckUserSubscriptionResponseModel
            {
                HasSubscription = false,
                CompanyId = company.Id,
                CompanyName = company.CompanyName ?? $"{company.FirstName} {company.LastName}",
                Message = "No subscription assigned to company",
            };
        }

        if (!company.SubscriptionActive)
        {
            return new CheckUserSubscriptionResponseModel
            {
                HasSubscription = false,
                IsExpired = true,
                CompanyId = company.Id,
                CompanyName = company.CompanyName ?? $"{company.FirstName} {company.LastName}",
                SubscriptionId = company.SubscriptionId,
                SubscriptionPlanName = company.Subscription?.PlanName,
                SubscriptionStartDate = company.SubscriptionStartDate,
                SubscriptionEndDate = company.SubscriptionEndDate,
                Message = "Subscription is inactive",
            };
        }

        // Check if subscription has expired
        if (
            company.SubscriptionEndDate.HasValue
            && company.SubscriptionEndDate.Value < DateTimeOffset.UtcNow
        )
        {
            return new CheckUserSubscriptionResponseModel
            {
                HasSubscription = false,
                IsExpired = true,
                CompanyId = company.Id,
                CompanyName = company.CompanyName ?? $"{company.FirstName} {company.LastName}",
                SubscriptionId = company.SubscriptionId,
                SubscriptionPlanName = company.Subscription?.PlanName,
                SubscriptionStartDate = company.SubscriptionStartDate,
                SubscriptionEndDate = company.SubscriptionEndDate,
                Message = $"Subscription expired on {company.SubscriptionEndDate:yyyy-MM-dd}",
            };
        }

        // Calculate days remaining
        int? daysRemaining = null;
        if (company.SubscriptionEndDate.HasValue)
        {
            daysRemaining = (int)
                (company.SubscriptionEndDate.Value - DateTimeOffset.UtcNow).TotalDays;
        }

        return new CheckUserSubscriptionResponseModel
        {
            HasSubscription = true,
            IsExpired = false,
            CompanyId = company.Id,
            CompanyName = company.CompanyName ?? $"{company.FirstName} {company.LastName}",
            SubscriptionId = company.SubscriptionId,
            SubscriptionPlanName = company.Subscription?.PlanName,
            SubscriptionStartDate = company.SubscriptionStartDate,
            SubscriptionEndDate = company.SubscriptionEndDate,
            DaysRemaining = daysRemaining,
            MaxUsersAllowed = company.Subscription?.MaxUsersAllowed,
            MaxBranches = company.Subscription?.MaxBranches,
            Message =
                daysRemaining.HasValue && daysRemaining < 30
                    ? $"Subscription expires in {daysRemaining} days"
                    : "Subscription is active",
        };
    }
}

public class CheckUserSubscriptionResponseModel
{
    public bool HasSubscription { get; set; }
    public bool IsExpired { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string SubscriptionId { get; set; }
    public string SubscriptionPlanName { get; set; }
    public DateTimeOffset? SubscriptionStartDate { get; set; }
    public DateTimeOffset? SubscriptionEndDate { get; set; }
    public int? DaysRemaining { get; set; }
    public int? MaxUsersAllowed { get; set; }
    public int? MaxBranches { get; set; }
    public string Message { get; set; }
}
