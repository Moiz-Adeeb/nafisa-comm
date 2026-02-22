using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.CompanyRoles.Models;
using Domain.Constant;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyRoles.Commands;

public class UpdateCompanyRoleRequestModel : IRequest<UpdateCompanyRoleResponseModel>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}

public class UpdateCompanyRoleRequestModelValidator
    : AbstractValidator<UpdateCompanyRoleRequestModel>
{
    public UpdateCompanyRoleRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.Name).Required().Max(100);
        RuleFor(p => p.Description).Max(500);
        RuleFor(p => p.Permissions)
            .Must(permissions =>
                permissions == null
                || permissions.All(p => ClaimNames.AllCompanyAdminClaims.Contains(p))
            )
            .WithMessage("All permissions must be valid company claims");
    }
}

public class UpdateCompanyRoleRequestHandler
    : IRequestHandler<UpdateCompanyRoleRequestModel, UpdateCompanyRoleResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public UpdateCompanyRoleRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<UpdateCompanyRoleResponseModel> Handle(
        UpdateCompanyRoleRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var roleName = request.Name.Trim();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can update company roles."
            );
        }

        // Get existing role
        var companyRole = await _context
            .CompanyRoles.Include(r => r.CompanyRoleClaims)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (companyRole == null)
        {
            throw new NotFoundException("Company role not found");
        }

        // Validate tenant ownership
        if (companyRole.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot update company role from a different company");
        }

        // Check if name already exists within the company (excluding current role)
        var nameExists = await _context.CompanyRoles.AnyAsync(
            r =>
                r.CompanyId == companyId
                && r.Name == roleName
                && r.Id != request.Id
                && !r.IsDeleted,
            cancellationToken
        );

        if (nameExists)
        {
            throw new AlreadyExistsException($"Company role with name '{roleName}' already exists");
        }

        // Update basic properties
        companyRole.Name = roleName;
        companyRole.Description = request.Description?.Trim();
        companyRole.IsActive = request.IsActive;

        // Remove all existing claims
        if (companyRole.CompanyRoleClaims != null && companyRole.CompanyRoleClaims.Any())
        {
            _context.CompanyRoleClaims.RemoveRange(companyRole.CompanyRoleClaims);
        }

        // Add new permissions as claims
        companyRole.CompanyRoleClaims = new List<Domain.Entities.CompanyRoleClaim>();
        if (request.Permissions != null && request.Permissions.Any())
        {
            foreach (var permission in request.Permissions.Distinct())
            {
                companyRole.CompanyRoleClaims.Add(
                    new Domain.Entities.CompanyRoleClaim
                    {
                        ClaimType = "Permission",
                        ClaimValue = permission,
                    }
                );
            }
        }

        _context.CompanyRoles.Update(companyRole);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description =
                    $"Company role '{companyRole.Name}' updated with {request.Permissions?.Count ?? 0} permissions",
                DescriptionFr =
                    $"Rôle d'entreprise '{companyRole.Name}' mis à jour avec {request.Permissions?.Count ?? 0} permissions",
                EntityId = companyRole.Id,
            }
        );

        var dto = new CompanyRoleDto(companyRole) { Permissions = request.Permissions };

        return new UpdateCompanyRoleResponseModel { Data = dto };
    }
}

public class UpdateCompanyRoleResponseModel
{
    public CompanyRoleDto Data { get; set; }
}
