using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.LoanPayments.Commands;

public class PayBackLoanRequestModel : IRequest<PayBackLoanResponseModel>
{
    public string LoanPaymentScheduleId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentProof { get; set; } // Base64 image or document (optional)
}

public class PayBackLoanRequestModelValidator : AbstractValidator<PayBackLoanRequestModel>
{
    public PayBackLoanRequestModelValidator()
    {
        RuleFor(p => p.LoanPaymentScheduleId).Required();
        RuleFor(p => p.Amount).Required().GreaterThan(0);
    }
}

public class PayBackLoanRequestHandler
    : IRequestHandler<PayBackLoanRequestModel, PayBackLoanResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public PayBackLoanRequestHandler(
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

    public async Task<PayBackLoanResponseModel> Handle(
        PayBackLoanRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        // Get the loan payment schedule
        var loanPaymentSchedule = await _context
            .Set<LoanPaymentSchedule>()
            .Include(lps => lps.LoanRequest)
            .Include(lps => lps.CompanyStaff)
            .ThenInclude(cs => cs.User)
            .FirstOrDefaultAsync(
                lps =>
                    lps.Id == request.LoanPaymentScheduleId
                    && lps.CompanyId == companyId
                    && !lps.IsDeleted,
                cancellationToken
            );

        if (loanPaymentSchedule == null)
        {
            throw new NotFoundException("Loan payment schedule not found");
        }

        // Verify the loan payment schedule belongs to the current user (for employee access)
        // or allow company admin to make payments on behalf of employees
        var currentUser = await _context.Users.FindAsync(userId);
        var isEmployee = loanPaymentSchedule.CompanyStaffId == userId;
        var isCompanyAdmin = !string.IsNullOrWhiteSpace(companyId); // Simple check, can be enhanced

        if (!isEmployee && !isCompanyAdmin)
        {
            throw new BadRequestException(
                "You do not have permission to make this payment"
            );
        }

        if (loanPaymentSchedule.IsPayed)
        {
            throw new AlreadyExistsException("This loan payment has already been paid");
        }

        // Validate payment amount matches the scheduled amount
        if (request.Amount != loanPaymentSchedule.Amount)
        {
            throw new BadRequestException(
                $"Payment amount ({request.Amount:C}) does not match the scheduled amount ({loanPaymentSchedule.Amount:C})"
            );
        }

        // Save payment proof if provided
        string paymentProofPath = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentProof))
        {
            paymentProofPath = await _imageService.SaveImageToServer(
                request.PaymentProof,
                ".png",
                "loan-payment-proofs"
            );
        }

        // Mark as paid
        loanPaymentSchedule.IsPayed = true;
        loanPaymentSchedule.PaidDate = DateTimeOffset.UtcNow;

        // Create transaction record
        var transaction = new Transaction
        {
            CompanyId = companyId,
            CompanyStaffId = loanPaymentSchedule.CompanyStaffId,
            Type = TransactionType.LoanPayment,
            Amount = request.Amount,
            Description =
                $"Loan payment for {loanPaymentSchedule.Month}/{loanPaymentSchedule.Year} - Loan: {loanPaymentSchedule.LoanRequest?.Title}",
            DescriptionFr =
                $"Paiement de prêt pour {loanPaymentSchedule.Month}/{loanPaymentSchedule.Year} - Prêt: {loanPaymentSchedule.LoanRequest?.Title}",
            Reference =
                $"LOAN-PAY-{loanPaymentSchedule.Month:D2}{loanPaymentSchedule.Year}-{loanPaymentSchedule.CompanyStaffId.Substring(0, 8)}",
            LoanPaymentScheduleId = loanPaymentSchedule.Id,
            TransactionDate = DateTimeOffset.UtcNow,
        };

        _context.Set<LoanPaymentSchedule>().Update(loanPaymentSchedule);
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description =
                    $"Loan payment made for '{loanPaymentSchedule.CompanyStaff?.User?.FullName}' - Amount: {request.Amount:C}",
                DescriptionFr =
                    $"Paiement de prêt effectué pour '{loanPaymentSchedule.CompanyStaff?.User?.FullName}' - Montant: {request.Amount:C}",
                EntityId = loanPaymentSchedule.Id,
            }
        );

        return new PayBackLoanResponseModel
        {
            Success = true,
            Message = "Loan payment processed successfully",
            TransactionId = transaction.Id,
            PaidAmount = request.Amount,
            PaymentDate = loanPaymentSchedule.PaidDate.Value,
        };
    }
}

public class PayBackLoanResponseModel
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string TransactionId { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
}