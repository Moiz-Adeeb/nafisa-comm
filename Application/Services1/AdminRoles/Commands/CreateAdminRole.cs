using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AdminRoles.Models;
using Application.Services.AuditLogs.Commands;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.AdminRoles.Commands;

public class CreateAdminRoleRequestModel : IRequest<CreateAdminRoleResponseModel>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}

public class CreateAdminRoleRequestModelValidator : AbstractValidator<CreateAdminRoleRequestModel>
{
    public CreateAdminRoleRequestModelValidator()
    {
        RuleFor(p => p.Name).Required().Max(100);
        RuleFor(p => p.Description).Max(500);
        RuleFor(p => p.Permissions)
            .Must(permissions =>
                permissions == null || permissions.All(p => ClaimNames.AllAdminClaims.Contains(p))
            )
            .WithMessage("All permissions must be valid admin claims");
    }
}

public class CreateAdminRoleRequestHandler
    : IRequestHandler<CreateAdminRoleRequestModel, CreateAdminRoleResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public CreateAdminRoleRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<CreateAdminRoleResponseModel> Handle(
        CreateAdminRoleRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var roleName = request.Name.Trim();

        // Check if role already exists
        var existingRole = await _context.AdminRoles.AnyAsync(
            r => r.Name == roleName && !r.IsDeleted,
            cancellationToken
        );

        if (existingRole)
        {
            throw new AlreadyExistsException($"Admin role with name '{roleName}' already exists");
        }

        // Create role
        var adminRole = new AdminRole
        {
            Name = roleName,
            Description = request.Description?.Trim(),
            IsActive = true,
            AdminRoleClaims = new List<AdminRoleClaim>(),
        };

        // Add permissions as claims
        if (request.Permissions != null && request.Permissions.Any())
        {
            foreach (var permission in request.Permissions.Distinct())
            {
                adminRole.AdminRoleClaims.Add(
                    new AdminRoleClaim { ClaimType = "Permission", ClaimValue = permission }
                );
            }
        }

        await _context.AdminRoles.AddAsync(adminRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description =
                    $"Admin role '{adminRole.Name}' created with {request.Permissions?.Count ?? 0} permissions",
                DescriptionFr =
                    $"Rôle administrateur '{adminRole.Name}' créé avec {request.Permissions?.Count ?? 0} permissions",
                EntityId = adminRole.Id,
            }
        );

        var dto = new AdminRoleDto(adminRole) { Permissions = request.Permissions };

        return new CreateAdminRoleResponseModel { Data = dto };
    }
}

public class CreateAdminRoleResponseModel
{
    public AdminRoleDto Data { get; set; }
}
