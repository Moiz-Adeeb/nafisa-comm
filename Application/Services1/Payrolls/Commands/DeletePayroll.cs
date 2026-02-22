using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Payrolls.Commands;

public class DeletePayrollRequestModel : IRequest<DeletePayrollResponseModel>
{
    public string Id { get; set; }
}

public class DeletePayrollRequestModelValidator : AbstractValidator<DeletePayrollRequestModel>
{
    public DeletePayrollRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeletePayrollRequestHandler
    : IRequestHandler<DeletePayrollRequestModel, DeletePayrollResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeletePayrollRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeletePayrollResponseModel> Handle(
        DeletePayrollRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can delete payroll."
            );
        }

        // Get existing payroll
        var payroll = await _context
            .PayRolls.Include(p => p.CompanyStaff)
            .ThenInclude(cs => cs.User)
            .FirstOrDefaultAsync(
                p => p.Id == request.Id && p.CompanyId == companyId && !p.IsDeleted,
                cancellationToken
            );

        if (payroll == null)
        {
            throw new NotFoundException("Payroll not found");
        }

        // Soft delete
        payroll.IsDeleted = true;
        _context.PayRolls.Update(payroll);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Delete,
                Description =
                    $"Payroll deleted for '{payroll.CompanyStaff?.User?.FullName}' for {payroll.Month}/{payroll.Year}",
                DescriptionFr =
                    $"Paie supprimée pour '{payroll.CompanyStaff?.User?.FullName}' pour {payroll.Month}/{payroll.Year}",
                EntityId = payroll.Id,
            }
        );

        return new DeletePayrollResponseModel { Success = true };
    }
}

public class DeletePayrollResponseModel
{
    public bool Success { get; set; }
}