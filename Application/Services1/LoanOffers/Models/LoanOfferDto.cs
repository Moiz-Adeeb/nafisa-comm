using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.LoanOffers.Models;

public class LoanOfferDto
{
    public string Id { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string Title { get; set; }
    public decimal InterestRate { get; set; }
    public decimal LoanMin { get; set; }
    public decimal LoanMax { get; set; }
    public List<int> Durations { get; set; } = new List<int>();
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int LoanRequestCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }

    public LoanOfferDto() { }

    public LoanOfferDto(LoanOffer offer)
    {
        Id = offer.Id;
        CompanyId = offer.CompanyId;
        Title = offer.Title;
        InterestRate = offer.InterestRate;
        LoanMin = offer.LoanMin;
        LoanMax = offer.LoanMax;
        Description = offer.Description;
        IsActive = offer.IsActive;
        CreatedDate = offer.CreatedDate;
        UpdatedDate = offer.UpdatedDate;

        // Parse durations from comma-separated string
        if (!string.IsNullOrWhiteSpace(offer.Durations))
        {
            Durations = offer
                .Durations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => int.TryParse(d.Trim(), out var duration) ? duration : 0)
                .Where(d => d > 0)
                .ToList();
        }
    }
}

public class LoanOfferSelector
{
    public static readonly Expression<Func<LoanOffer, LoanOfferDto>> Selector =
        p => new LoanOfferDto()
        {
            Id = p.Id,
            CompanyId = p.CompanyId,
            CompanyName =
                p.Company != null
                    ? p.Company.CompanyName ?? $"{p.Company.FirstName} {p.Company.LastName}"
                    : null,
            Title = p.Title,
            InterestRate = p.InterestRate,
            LoanMin = p.LoanMin,
            LoanMax = p.LoanMax,
            Description = p.Description,
            IsActive = p.IsActive,
            LoanRequestCount = p.LoanRequests != null ? p.LoanRequests.Count() : 0,
            CreatedDate = p.CreatedDate,
            UpdatedDate = p.UpdatedDate,
            // Note: Durations need to be parsed separately as EF Core can't translate string split in expressions
        };

    public static readonly Expression<Func<LoanOffer, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.Title };
}
