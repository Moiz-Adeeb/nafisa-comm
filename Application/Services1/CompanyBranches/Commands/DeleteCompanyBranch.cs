using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyBranches.Commands;

public class DeleteCompanyBranchRequestModel : IRequest<DeleteCompanyBranchResponseModel>
{
    public string Id { get; set; }
}

public class DeleteCompanyBranchRequestModelValidator
    : AbstractValidator<DeleteCompanyBranchRequestModel>
{
    public DeleteCompanyBranchRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteCompanyBranchRequestHandler
    : IRequestHandler<DeleteCompanyBranchRequestModel, DeleteCompanyBranchResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteCompanyBranchRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteCompanyBranchResponseModel> Handle(
        DeleteCompanyBranchRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can delete branches."
            );
        }

        // Get existing branch
        var branch = await _context.CompanyBranches.FirstOrDefaultAsync(
            b => b.Id == request.Id && !b.IsDeleted,
            cancellationToken
        );

        if (branch == null)
        {
            throw new NotFoundException("Branch not found");
        }

        // Validate tenant ownership
        if (branch.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot delete branch from a different company");
        }

        // Check if branch has staff assigned
        var hasStaff = await _context.CompanyStaffs.ActiveAny(
            s => s.BranchId == request.Id,
            cancellationToken
        );

        if (hasStaff)
        {
            throw new BadRequestException(
                "Cannot delete branch that has staff assigned. Please reassign or remove all staff first."
            );
        }

        // Soft delete
        branch.IsDeleted = true;
        _context.CompanyBranches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Delete,
                Description = $"Branch '{branch.Name}' deleted",
                DescriptionFr = $"Succursale '{branch.Name}' supprimée",
                EntityId = branch.Id,
            }
        );

        return new DeleteCompanyBranchResponseModel { Success = true };
    }
}

public class DeleteCompanyBranchResponseModel
{
    public bool Success { get; set; }
}
