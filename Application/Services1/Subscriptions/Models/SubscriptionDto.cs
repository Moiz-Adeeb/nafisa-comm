using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Subscriptions.Models;

public class SubscriptionDto
{
    public string Id { get; set; }
    public string PlanName { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public string BillingCycleName { get; set; }
    public decimal Price { get; set; }
    public int MaxUsersAllowed { get; set; }
    public int MaxBranches { get; set; }
    public string Features { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }

    public SubscriptionDto() { }

    public SubscriptionDto(Subscription subscription)
    {
        Id = subscription.Id;
        PlanName = subscription.PlanName;
        BillingCycle = subscription.BillingCycle;
        BillingCycleName = subscription.BillingCycle.ToString();
        Price = subscription.Price;
        MaxUsersAllowed = subscription.MaxUsersAllowed;
        MaxBranches = subscription.MaxBranches;
        Features = subscription.Features;
        IsActive = subscription.IsActive;
        CreatedDate = subscription.CreatedDate;
        UpdatedDate = subscription.UpdatedDate;
    }
}

public class SubscriptionSelector
{
    public static readonly Expression<Func<Subscription, SubscriptionDto>> Selector =
        p => new SubscriptionDto()
        {
            Id = p.Id,
            PlanName = p.PlanName,
            BillingCycle = p.BillingCycle,
            BillingCycleName = p.BillingCycle.ToString(),
            Price = p.Price,
            MaxUsersAllowed = p.MaxUsersAllowed,
            MaxBranches = p.MaxBranches,
            Features = p.Features,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate,
            UpdatedDate = p.UpdatedDate,
        };

    public static readonly Expression<Func<Subscription, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.PlanName };
}
