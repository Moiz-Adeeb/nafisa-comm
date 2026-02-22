using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.LoanRequests.Models;

public class LoanRequestDto
{
    public string Id { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }

    public string CompanyStaffId { get; set; }
    public string CompanyStaffName { get; set; }
    public string CompanyStaffEmail { get; set; }

    public string LoanOfferId { get; set; }
    public string LoanOfferTitle { get; set; }

    public string Title { get; set; }
    public decimal Amount { get; set; }
    public int Duration { get; set; }
    public decimal InterestRate { get; set; }
    public decimal TotalPayback { get; set; }
    public decimal MonthlyPayment { get; set; }

    public string Purpose { get; set; }
    public string DocumentPath { get; set; }

    public LoanRequestStatus Status { get; set; }
    public string StatusText { get; set; }

    // Approval/Rejection info
    public string ApprovedByUserId { get; set; }
    public string ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedDate { get; set; }

    public string RejectedByUserId { get; set; }
    public string RejectedByName { get; set; }
    public DateTimeOffset? RejectedDate { get; set; }
    public string RejectionReason { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }

    public LoanRequestDto() { }

    public LoanRequestDto(LoanRequest request)
    {
        Id = request.Id;
        CompanyId = request.CompanyId;
        CompanyStaffId = request.CompanyStaffId;
        LoanOfferId = request.LoanOfferId;
        Title = request.Title;
        Amount = request.Amount;
        Duration = request.Duration;
        InterestRate = request.InterestRate;
        TotalPayback = request.TotalPayback;
        MonthlyPayment = request.MonthlyPayment;
        Purpose = request.Purpose;
        DocumentPath = request.DocumentPath;
        Status = request.Status;
        StatusText = request.Status.ToString();
        ApprovedByUserId = request.ApprovedByUserId;
        ApprovedDate = request.ApprovedDate;
        RejectedByUserId = request.RejectedByUserId;
        RejectedDate = request.RejectedDate;
        RejectionReason = request.RejectionReason;
        CreatedDate = request.CreatedDate;
        UpdatedDate = request.UpdatedDate;
    }
}

public class LoanRequestSelector
{
    public static readonly Expression<Func<LoanRequest, LoanRequestDto>> Selector =
        p => new LoanRequestDto()
        {
            Id = p.Id,
            CompanyId = p.CompanyId,
            CompanyName =
                p.Company != null
                    ? p.Company.CompanyName ?? $"{p.Company.FirstName} {p.Company.LastName}"
                    : null,
            CompanyStaffId = p.CompanyStaffId,
            CompanyStaffName = p.CompanyStaff != null ? p.CompanyStaff.User.FullName : null,
            CompanyStaffEmail = p.CompanyStaff != null ? p.CompanyStaff.User.Email : null,
            LoanOfferId = p.LoanOfferId,
            LoanOfferTitle = p.LoanOffer != null ? p.LoanOffer.Title : null,
            Title = p.Title,
            Amount = p.Amount,
            Duration = p.Duration,
            InterestRate = p.InterestRate,
            TotalPayback = p.TotalPayback,
            MonthlyPayment = p.MonthlyPayment,
            Purpose = p.Purpose,
            DocumentPath = p.DocumentPath,
            Status = p.Status,
            StatusText = p.Status.ToString(),
            ApprovedByUserId = p.ApprovedByUserId,
            ApprovedByName = p.ApprovedBy != null ? p.ApprovedBy.FullName : null,
            ApprovedDate = p.ApprovedDate,
            RejectedByUserId = p.RejectedByUserId,
            RejectedByName = p.RejectedBy != null ? p.RejectedBy.FullName : null,
            RejectedDate = p.RejectedDate,
            RejectionReason = p.RejectionReason,
            CreatedDate = p.CreatedDate,
            UpdatedDate = p.UpdatedDate,
        };

    public static readonly Expression<Func<LoanRequest, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Id = p.Id, Name = p.Title };
}
