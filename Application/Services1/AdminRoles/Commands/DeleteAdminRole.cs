using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.AdminRoles.Commands;

public class DeleteAdminRoleRequestModel : IRequest<DeleteAdminRoleResponseModel>
{
    public string Id { get; set; }
}

public class DeleteAdminRoleRequestModelValidator : AbstractValidator<DeleteAdminRoleRequestModel>
{
    public DeleteAdminRoleRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteAdminRoleRequestHandler
    : IRequestHandler<DeleteAdminRoleRequestModel, DeleteAdminRoleResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteAdminRoleRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteAdminRoleResponseModel> Handle(
        DeleteAdminRoleRequestModel request,
        CancellationToken cancellationToken
    )
    {
        // Get existing role
        var adminRole = await _context.AdminRoles.FirstOrDefaultAsync(
            r => r.Id == request.Id && !r.IsDeleted,
            cancellationToken
        );

        if (adminRole == null)
        {
            throw new NotFoundException("Admin role not found");
        }

        // Check if role is assigned to any users
        var hasUsers = await _context.Users.AnyAsync(
            u => u.AdminRoleId == request.Id && !u.IsDeleted,
            cancellationToken
        );

        if (hasUsers)
        {
            throw new BadRequestException(
                "Cannot delete admin role that is assigned to users. Please unassign all users first."
            );
        }

        // Soft delete
        adminRole.IsDeleted = true;
        _context.AdminRoles.Update(adminRole);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Delete,
                Description = $"Admin role '{adminRole.Name}' deleted",
                DescriptionFr = $"Rôle administrateur '{adminRole.Name}' supprimé",
                EntityId = adminRole.Id,
            }
        );

        return new DeleteAdminRoleResponseModel { Success = true };
    }
}

public class DeleteAdminRoleResponseModel
{
    public bool Success { get; set; }
}
