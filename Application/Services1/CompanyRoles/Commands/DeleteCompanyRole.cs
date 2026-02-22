using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyRoles.Commands;

public class DeleteCompanyRoleRequestModel : IRequest<DeleteCompanyRoleResponseModel>
{
    public string Id { get; set; }
}

public class DeleteCompanyRoleRequestModelValidator
    : AbstractValidator<DeleteCompanyRoleRequestModel>
{
    public DeleteCompanyRoleRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteCompanyRoleRequestHandler
    : IRequestHandler<DeleteCompanyRoleRequestModel, DeleteCompanyRoleResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteCompanyRoleRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteCompanyRoleResponseModel> Handle(
        DeleteCompanyRoleRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can delete company roles."
            );
        }

        // Get existing role
        var companyRole = await _context.CompanyRoles.FirstOrDefaultAsync(
            r => r.Id == request.Id && !r.IsDeleted,
            cancellationToken
        );

        if (companyRole == null)
        {
            throw new NotFoundException("Company role not found");
        }

        // Validate tenant ownership
        if (companyRole.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot delete company role from a different company");
        }

        // Check if role is assigned to any users
        var hasUsers = await _context.Users.AnyAsync(
            u => u.CompanyStaff.CompanyRoleId == request.Id && !u.IsDeleted,
            cancellationToken
        );

        if (hasUsers)
        {
            throw new BadRequestException(
                "Cannot delete company role that is assigned to users. Please unassign all users first."
            );
        }

        // Soft delete
        companyRole.IsDeleted = true;
        _context.CompanyRoles.Update(companyRole);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Delete,
                Description = $"Company role '{companyRole.Name}' deleted",
                DescriptionFr = $"Rôle d'entreprise '{companyRole.Name}' supprimé",
                EntityId = companyRole.Id,
            }
        );

        return new DeleteCompanyRoleResponseModel { Success = true };
    }
}

public class DeleteCompanyRoleResponseModel
{
    public bool Success { get; set; }
}
