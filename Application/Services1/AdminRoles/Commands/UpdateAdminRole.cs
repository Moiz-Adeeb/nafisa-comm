using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AdminRoles.Models;
using Application.Services.AuditLogs.Commands;
using Domain.Constant;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.AdminRoles.Commands;

public class UpdateAdminRoleRequestModel : IRequest<UpdateAdminRoleResponseModel>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}

public class UpdateAdminRoleRequestModelValidator : AbstractValidator<UpdateAdminRoleRequestModel>
{
    public UpdateAdminRoleRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.Name).Required().Max(100);
        RuleFor(p => p.Description).Max(500);
        RuleFor(p => p.Permissions)
            .Must(permissions =>
                permissions == null || permissions.All(p => ClaimNames.AllAdminClaims.Contains(p))
            )
            .WithMessage("All permissions must be valid admin claims");
    }
}

public class UpdateAdminRoleRequestHandler
    : IRequestHandler<UpdateAdminRoleRequestModel, UpdateAdminRoleResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public UpdateAdminRoleRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<UpdateAdminRoleResponseModel> Handle(
        UpdateAdminRoleRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var roleName = request.Name.Trim();

        // Get existing role
        var adminRole = await _context
            .AdminRoles.Include(r => r.AdminRoleClaims)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (adminRole == null)
        {
            throw new NotFoundException("Admin role not found");
        }

        // Check if name already exists (excluding current role)
        var nameExists = await _context.AdminRoles.AnyAsync(
            r => r.Name == roleName && r.Id != request.Id && !r.IsDeleted,
            cancellationToken
        );

        if (nameExists)
        {
            throw new AlreadyExistsException($"Admin role with name '{roleName}' already exists");
        }

        // Update basic properties
        adminRole.Name = roleName;
        adminRole.Description = request.Description?.Trim();
        adminRole.IsActive = request.IsActive;

        // Remove all existing claims
        if (adminRole.AdminRoleClaims != null && adminRole.AdminRoleClaims.Any())
        {
            _context.AdminRoleClaims.RemoveRange(adminRole.AdminRoleClaims);
        }

        // Add new permissions as claims
        adminRole.AdminRoleClaims = new List<Domain.Entities.AdminRoleClaim>();
        if (request.Permissions != null && request.Permissions.Any())
        {
            foreach (var permission in request.Permissions.Distinct())
            {
                adminRole.AdminRoleClaims.Add(
                    new Domain.Entities.AdminRoleClaim
                    {
                        ClaimType = "Permission",
                        ClaimValue = permission,
                    }
                );
            }
        }

        _context.AdminRoles.Update(adminRole);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Admin role '{adminRole.Name}' updated with {request.Permissions?.Count ?? 0} permissions",
                DescriptionFr =
                    $"Rôle administrateur '{adminRole.Name}' mis à jour avec {request.Permissions?.Count ?? 0} permissions",
                EntityId = adminRole.Id,
            }
        );

        var dto = new AdminRoleDto(adminRole) { Permissions = request.Permissions };

        return new UpdateAdminRoleResponseModel { Data = dto };
    }
}

public class UpdateAdminRoleResponseModel
{
    public AdminRoleDto Data { get; set; }
}
