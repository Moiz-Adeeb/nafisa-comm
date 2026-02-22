using System.Linq.Expressions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Payrolls.Models;

public class PayrollDto
{
    public string Id { get; set; }
    public string CompanyId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string CompanyStaffId { get; set; }
    public string StaffName { get; set; }
    public string StaffEmail { get; set; }
    public string BranchId { get; set; }
    public string BranchName { get; set; }
    public decimal Amount { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal NetAmount { get; set; }
    public PayrollStatus Status { get; set; }
    public string ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedDate { get; set; }
    public string ApprovalProof { get; set; }
    public string RejectionReason { get; set; }
    public DateTimeOffset? RejectedDate { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public PayrollDto() { }

    public PayrollDto(PayRoll payroll)
    {
        Id = payroll.Id;
        CompanyId = payroll.CompanyId;
        Month = payroll.Month;
        Year = payroll.Year;
        CompanyStaffId = payroll.CompanyStaffId;
        StaffName = payroll.CompanyStaff?.User?.FullName;
        StaffEmail = payroll.CompanyStaff?.User?.Email;
        BranchId = payroll.CompanyStaff?.BranchId;
        BranchName = payroll.CompanyStaff?.Branch?.Name;
        Amount = payroll.Amount;
        LoanDeduction = payroll.LoanDeduction;
        NetAmount = payroll.NetAmount;
        Status = payroll.Status;
        ApprovedByUserId = payroll.ApprovedByUserId;
        ApprovedDate = payroll.ApprovedDate;
        ApprovalProof = payroll.ApprovalProof;
        RejectionReason = payroll.RejectionReason;
        RejectedDate = payroll.RejectedDate;
        CreatedDate = payroll.CreatedDate;
    }
}

public class PayrollSelector
{
    public static readonly Expression<Func<PayRoll, PayrollDto>> Selector = p =>
        new PayrollDto()
        {
            Id = p.Id,
            CompanyId = p.CompanyId,
            Month = p.Month,
            Year = p.Year,
            CompanyStaffId = p.CompanyStaffId,
            StaffName = p.CompanyStaff.User.FullName,
            StaffEmail = p.CompanyStaff.User.Email,
            BranchId = p.CompanyStaff.BranchId,
            BranchName = p.CompanyStaff.Branch.Name,
            Amount = p.Amount,
            LoanDeduction = p.LoanDeduction,
            NetAmount = p.NetAmount,
            Status = p.Status,
            ApprovedByUserId = p.ApprovedByUserId,
            ApprovedDate = p.ApprovedDate,
            ApprovalProof = p.ApprovalProof,
            RejectionReason = p.RejectionReason,
            RejectedDate = p.RejectedDate,
            CreatedDate = p.CreatedDate,
        };
}