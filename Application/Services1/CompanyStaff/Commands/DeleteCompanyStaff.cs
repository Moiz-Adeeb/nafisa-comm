using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Constant;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyStaff.Commands;

public class DeleteCompanyStaffRequestModel : IRequest<DeleteCompanyStaffResponseModel>
{
    public string Id { get; set; }
}

public class DeleteCompanyStaffRequestModelValidator
    : AbstractValidator<DeleteCompanyStaffRequestModel>
{
    public DeleteCompanyStaffRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteCompanyStaffRequestHandler
    : IRequestHandler<DeleteCompanyStaffRequestModel, DeleteCompanyStaffResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteCompanyStaffRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteCompanyStaffResponseModel> Handle(
        DeleteCompanyStaffRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can delete company staff."
            );
        }

        // Get existing user
        var user = await _context
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Company staff not found");
        }

        // Verify user is company staff (has CompanyAdmin or Employee role)
        var isCompanyStaff = user.UserRoles.Any(ur =>
            ur.Role.Name == RoleNames.CompanyAdmin || ur.Role.Name == RoleNames.Employee
        );

        if (!isCompanyStaff)
        {
            throw new BadRequestException("User is not a company staff member");
        }

        // Tenant validation: verify user belongs to same company
        // if (user.CompanyId != companyId) // If entity supports CompanyId
        // {
        //     throw new BadRequestException("Cannot delete staff from a different company");
        // }

        // Prevent self-deletion
        var currentUserId = _sessionService.GetUserId();
        if (user.Id == currentUserId)
        {
            throw new BadRequestException("You cannot delete your own account");
        }

        // Soft delete
        user.IsDeleted = true;
        user.IsEnabled = false;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = currentUserId,
                Feature = AuditLogFeatureType.User,
                Action = AuditLogType.Delete,
                Description = $"Company staff '{user.FullName}' deleted",
                DescriptionFr = $"Personnel de l'entreprise '{user.FullName}' supprimé",
                EntityId = user.Id,
            }
        );

        return new DeleteCompanyStaffResponseModel { Success = true };
    }
}

public class DeleteCompanyStaffResponseModel
{
    public bool Success { get; set; }
}
