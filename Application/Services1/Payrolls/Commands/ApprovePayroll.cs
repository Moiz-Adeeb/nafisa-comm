using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Payrolls.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Payrolls.Commands;

public class ApprovePayrollRequestModel : IRequest<ApprovePayrollResponseModel>
{
    public string Id { get; set; }
    public string ApprovalProof { get; set; } // Base64 image or document
}

public class ApprovePayrollRequestModelValidator : AbstractValidator<ApprovePayrollRequestModel>
{
    public ApprovePayrollRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.ApprovalProof).Required();
    }
}

public class ApprovePayrollRequestHandler
    : IRequestHandler<ApprovePayrollRequestModel, ApprovePayrollResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public ApprovePayrollRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IImageService imageService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _imageService = imageService;
    }

    public async Task<ApprovePayrollResponseModel> Handle(
        ApprovePayrollRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can approve payroll."
            );
        }

        // Get existing payroll
        var payroll = await _context
            .PayRolls.Include(p => p.CompanyStaff)
            .ThenInclude(cs => cs.User)
            .Include(p => p.CompanyStaff)
            .ThenInclude(cs => cs.Branch)
            .FirstOrDefaultAsync(
                p =>
                    p.Id == request.Id
                    && p.CompanyId == companyId
                    && !p.IsDeleted
                    && p.Status == PayrollStatus.Pending,
                cancellationToken
            );

        if (payroll == null)
        {
            throw new NotFoundException("Payroll not found or already processed");
        }

        // Save approval proof
        var approvalProofPath = await _imageService.SaveImageToServer(
            request.ApprovalProof,
            ".png",
            "payroll-proofs"
        );

        // Update payroll
        payroll.Status = PayrollStatus.Approved;
        payroll.ApprovedByUserId = userId;
        payroll.ApprovedDate = DateTimeOffset.UtcNow;
        payroll.ApprovalProof = approvalProofPath;

        // Create salary payment transaction
        var transaction = new Transaction
        {
            CompanyId = companyId,
            CompanyStaffId = payroll.CompanyStaffId,
            Type = TransactionType.SalaryPayment,
            Amount = payroll.NetAmount,
            Description =
                $"Salary payment for {payroll.Month}/{payroll.Year} - Net: {payroll.NetAmount:C}",
            DescriptionFr =
                $"Paiement de salaire pour {payroll.Month}/{payroll.Year} - Net: {payroll.NetAmount:C}",
            Reference = $"PAY-{payroll.Month:D2}{payroll.Year}-{payroll.CompanyStaffId.Substring(0, 8)}",
            PayRollId = payroll.Id,
            TransactionDate = DateTimeOffset.UtcNow,
        };

        // If there were loan deductions, create a loan deduction transaction
        if (payroll.LoanDeduction > 0)
        {
            var loanDeductionTransaction = new Transaction
            {
                CompanyId = companyId,
                CompanyStaffId = payroll.CompanyStaffId,
                Type = TransactionType.LoanDeduction,
                Amount = payroll.LoanDeduction,
                Description =
                    $"Loan deduction from salary {payroll.Month}/{payroll.Year} - Amount: {payroll.LoanDeduction:C}",
                DescriptionFr =
                    $"Déduction de prêt du salaire {payroll.Month}/{payroll.Year} - Montant: {payroll.LoanDeduction:C}",
                Reference = $"LOAN-DED-{payroll.Month:D2}{payroll.Year}-{payroll.CompanyStaffId.Substring(0, 8)}",
                PayRollId = payroll.Id,
                TransactionDate = DateTimeOffset.UtcNow,
            };
            await _context.Transactions.AddAsync(loanDeductionTransaction, cancellationToken);
        }

        _context.PayRolls.Update(payroll);
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Payroll approved for '{payroll.CompanyStaff?.User?.FullName}' for {payroll.Month}/{payroll.Year}",
                DescriptionFr =
                    $"Paie approuvée pour '{payroll.CompanyStaff?.User?.FullName}' pour {payroll.Month}/{payroll.Year}",
                EntityId = payroll.Id,
            }
        );

        var dto = new PayrollDto(payroll);

        return new ApprovePayrollResponseModel { Data = dto };
    }
}

public class ApprovePayrollResponseModel
{
    public PayrollDto Data { get; set; }
}