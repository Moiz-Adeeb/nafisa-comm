using System.Linq.Expressions;
using Application.Shared;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Companies.Models;

public class CompanyDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string CompanyName { get; set; }
    public string BusinessRegistrationCertificate { get; set; }
    public string VatCertificate { get; set; }
    public string AuthorizationIdProof { get; set; }
    public RequestStatus Status { get; set; }
    public string StatusName { get; set; }
    public string ApprovedByUserId { get; set; }
    public string ApprovedByUserName { get; set; }
    public DateTimeOffset? ApprovedDate { get; set; }
    public string RejectedByUserId { get; set; }
    public string RejectedByUserName { get; set; }
    public DateTimeOffset? RejectedDate { get; set; }
    public string AdminNotes { get; set; }
    public string SubscriptionId { get; set; }
    public string SubscriptionPlanName { get; set; }
    public DateTimeOffset? SubscriptionStartDate { get; set; }
    public DateTimeOffset? SubscriptionEndDate { get; set; }
    public bool SubscriptionActive { get; set; }
    public bool IsActive { get; set; }
    public string CompanyAdminUserId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }

    public CompanyDto() { }

    public CompanyDto(Company company)
    {
        Id = company.Id;
        FirstName = company.FirstName;
        LastName = company.LastName;
        Email = company.Email;
        PhoneNumber = company.PhoneNumber;
        CompanyName = company.CompanyName;
        BusinessRegistrationCertificate = company.BusinessRegistrationCertificate;
        VatCertificate = company.VatCertificate;
        AuthorizationIdProof = company.AuthorizationIdProof;
        Status = company.Status;
        StatusName = company.Status.ToString();
        ApprovedByUserId = company.ApprovedByUserId;
        ApprovedDate = company.ApprovedDate;
        RejectedByUserId = company.RejectedByUserId;
        RejectedDate = company.RejectedDate;
        AdminNotes = company.AdminNotes;
        SubscriptionId = company.SubscriptionId;
        SubscriptionStartDate = company.SubscriptionStartDate;
        SubscriptionEndDate = company.SubscriptionEndDate;
        SubscriptionActive = company.SubscriptionActive;
        IsActive = company.IsActive;
        CompanyAdminUserId = company.CompanyAdminUserId;
        CreatedDate = company.CreatedDate;
        UpdatedDate = company.UpdatedDate;
    }
}

public class CompanySelector
{
    public static readonly Expression<Func<Company, CompanyDto>> Selector = p => new CompanyDto()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Email = p.Email,
        PhoneNumber = p.PhoneNumber,
        CompanyName = p.CompanyName,
        BusinessRegistrationCertificate = p.BusinessRegistrationCertificate,
        VatCertificate = p.VatCertificate,
        AuthorizationIdProof = p.AuthorizationIdProof,
        Status = p.Status,
        StatusName = p.Status.ToString(),
        ApprovedByUserId = p.ApprovedByUserId,
        ApprovedByUserName = p.ApprovedByUser != null ? p.ApprovedByUser.FullName : null,
        ApprovedDate = p.ApprovedDate,
        RejectedByUserId = p.RejectedByUserId,
        RejectedByUserName = p.RejectedByUser != null ? p.RejectedByUser.FullName : null,
        RejectedDate = p.RejectedDate,
        AdminNotes = p.AdminNotes,
        SubscriptionId = p.SubscriptionId,
        SubscriptionPlanName = p.Subscription != null ? p.Subscription.PlanName : null,
        SubscriptionStartDate = p.SubscriptionStartDate,
        SubscriptionEndDate = p.SubscriptionEndDate,
        SubscriptionActive = p.SubscriptionActive,
        IsActive = p.IsActive,
        CompanyAdminUserId = p.CompanyAdminUserId,
        CreatedDate = p.CreatedDate,
        UpdatedDate = p.UpdatedDate,
    };

    public static readonly Expression<Func<Company, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>()
        {
            Id = p.Id,
            Name = p.CompanyName ?? p.FirstName + " " + p.LastName,
        };
}
