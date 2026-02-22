using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.LoanRequests.Models;
using Domain.Enums;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.LoanRequests.Commands;

public class RejectLoanRequestRequestModel : IRequest<RejectLoanRequestResponseModel>
{
    public string Id { get; set; }
    public string RejectionReason { get; set; }
}

public class RejectLoanRequestRequestModelValidator
    : AbstractValidator<RejectLoanRequestRequestModel>
{
    public RejectLoanRequestRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.RejectionReason).Required().Max(500);
    }
}

public class RejectLoanRequestRequestHandler
    : IRequestHandler<RejectLoanRequestRequestModel, RejectLoanRequestResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public RejectLoanRequestRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<RejectLoanRequestResponseModel> Handle(
        RejectLoanRequestRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can reject loan requests."
            );
        }

        // Get loan request
        var loanRequest = await _context
            .LoanRequests.Include(r => r.CompanyStaff.User)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (loanRequest == null)
        {
            throw new NotFoundException("Loan request not found");
        }

        // Validate tenant ownership
        if (loanRequest.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot reject loan request from a different company");
        }

        // Validate status
        if (loanRequest.Status != LoanRequestStatus.Pending)
        {
            throw new BadRequestException(
                $"Cannot reject loan request with status: {loanRequest.Status}"
            );
        }

        // Update loan request status
        loanRequest.Status = LoanRequestStatus.Rejected;
        loanRequest.RejectedByUserId = userId;
        loanRequest.RejectedDate = DateTimeOffset.UtcNow;
        loanRequest.RejectionReason = request.RejectionReason?.Trim();

        _context.LoanRequests.Update(loanRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Loan request '{loanRequest.Title}' rejected for employee '{loanRequest.CompanyStaff?.User.FullName}'",
                DescriptionFr =
                    $"Demande de prêt '{loanRequest.Title}' rejetée pour l'employé '{loanRequest.CompanyStaff?.User.FullName}'",
                EntityId = loanRequest.Id,
            }
        );

        // TODO: Send notification to employee about rejection

        var dto = new LoanRequestDto(loanRequest);

        return new RejectLoanRequestResponseModel { Data = dto };
    }
}

public class RejectLoanRequestResponseModel
{
    public LoanRequestDto Data { get; set; }
}
