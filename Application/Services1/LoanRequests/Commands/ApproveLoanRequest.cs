using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.LoanRequests.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.LoanRequests.Commands;

public class ApproveLoanRequestRequestModel : IRequest<ApproveLoanRequestResponseModel>
{
    public string Id { get; set; }
}

public class ApproveLoanRequestRequestModelValidator
    : AbstractValidator<ApproveLoanRequestRequestModel>
{
    public ApproveLoanRequestRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class ApproveLoanRequestRequestHandler
    : IRequestHandler<ApproveLoanRequestRequestModel, ApproveLoanRequestResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public ApproveLoanRequestRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<ApproveLoanRequestResponseModel> Handle(
        ApproveLoanRequestRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can approve loan requests."
            );
        }

        // Get loan request
        var loanRequest = await _context
            .LoanRequests.Include(r => r.CompanyStaff.User)
            .FirstOrDefaultAsync(
                r =>
                    r.Id == request.Id
                    && r.CompanyId == companyId
                    && r.Status == LoanRequestStatus.Pending,
                cancellationToken
            );

        if (loanRequest == null)
        {
            throw new NotFoundException("Loan request not found");
        }

        // Update loan request status
        loanRequest.Status = LoanRequestStatus.Approved;
        loanRequest.ApprovedByUserId = userId;
        loanRequest.ApprovedDate = DateTimeOffset.UtcNow;
        loanRequest.LoanPaymentSchedules = new List<LoanPaymentSchedule>() { };
        var currentDate = DateTimeOffset.UtcNow;
        for (int i = 0; i < loanRequest.Duration; i++)
        {
            var scheduleDate = currentDate.AddMonths(i + 1);
            loanRequest.LoanPaymentSchedules.Add(
                new LoanPaymentSchedule()
                {
                    CompanyStaffId = loanRequest.CompanyStaffId,
                    CompanyId = companyId,
                    LoanRequestId = loanRequest.Id,
                    Amount = loanRequest.MonthlyPayment,
                    Month = scheduleDate.Month,
                    Year = scheduleDate.Year,
                }
            );
        }
        _context.LoanRequests.Update(loanRequest);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Loan request '{loanRequest.Title}' approved for employee '{loanRequest.CompanyStaff?.User.FullName}'",
                DescriptionFr =
                    $"Demande de prêt '{loanRequest.Title}' approuvée pour l'employé '{loanRequest.CompanyStaff?.User.FullName}'",
                EntityId = loanRequest.Id,
            }
        );

        // TODO: Send notification to employee about approval

        var dto = new LoanRequestDto(loanRequest);

        return new ApproveLoanRequestResponseModel { Data = dto };
    }
}

public class ApproveLoanRequestResponseModel
{
    public LoanRequestDto Data { get; set; }
}
