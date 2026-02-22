using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Payrolls.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Payrolls.Commands;

public class RejectPayrollRequestModel : IRequest<RejectPayrollResponseModel>
{
    public string Id { get; set; }
    public string RejectionReason { get; set; }
}

public class RejectPayrollRequestModelValidator : AbstractValidator<RejectPayrollRequestModel>
{
    public RejectPayrollRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.RejectionReason).Required().Max(500);
    }
}

public class RejectPayrollRequestHandler
    : IRequestHandler<RejectPayrollRequestModel, RejectPayrollResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public RejectPayrollRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<RejectPayrollResponseModel> Handle(
        RejectPayrollRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can reject payroll."
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

        // Update payroll
        payroll.Status = PayrollStatus.Rejected;
        payroll.RejectionReason = request.RejectionReason;
        payroll.RejectedDate = DateTimeOffset.UtcNow;

        _context.PayRolls.Update(payroll);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Payroll rejected for '{payroll.CompanyStaff?.User?.FullName}' for {payroll.Month}/{payroll.Year}. Reason: {request.RejectionReason}",
                DescriptionFr =
                    $"Paie rejetée pour '{payroll.CompanyStaff?.User?.FullName}' pour {payroll.Month}/{payroll.Year}. Raison: {request.RejectionReason}",
                EntityId = payroll.Id,
            }
        );

        var dto = new PayrollDto(payroll);

        return new RejectPayrollResponseModel { Data = dto };
    }
}

public class RejectPayrollResponseModel
{
    public PayrollDto Data { get; set; }
}