using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.CompanyRoles.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyRoles.Commands;

public class CreateCompanyRoleRequestModel : IRequest<CreateCompanyRoleResponseModel>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}

public class CreateCompanyRoleRequestModelValidator
    : AbstractValidator<CreateCompanyRoleRequestModel>
{
    public CreateCompanyRoleRequestModelValidator()
    {
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

public class CreateCompanyRoleRequestHandler
    : IRequestHandler<CreateCompanyRoleRequestModel, CreateCompanyRoleResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public CreateCompanyRoleRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<CreateCompanyRoleResponseModel> Handle(
        CreateCompanyRoleRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var roleName = request.Name.Trim();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can create company roles."
            );
        }

        // Check if role already exists within the company
        var existingRole = await _context.CompanyRoles.AnyAsync(
            r => r.CompanyId == companyId && r.Name == roleName && !r.IsDeleted,
            cancellationToken
        );

        if (existingRole)
        {
            throw new AlreadyExistsException($"Company role with name '{roleName}' already exists");
        }

        // Create role
        var companyRole = new CompanyRole
        {
            CompanyId = companyId,
            Name = roleName,
            Description = request.Description?.Trim(),
            IsActive = true,
            CompanyRoleClaims = new List<CompanyRoleClaim>(),
        };

        // Add permissions as claims
        if (request.Permissions != null && request.Permissions.Any())
        {
            foreach (var permission in request.Permissions.Distinct())
            {
                companyRole.CompanyRoleClaims.Add(
                    new CompanyRoleClaim { ClaimType = "Permission", ClaimValue = permission }
                );
            }
        }

        await _context.CompanyRoles.AddAsync(companyRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description =
                    $"Company role '{companyRole.Name}' created with {request.Permissions?.Count ?? 0} permissions",
                DescriptionFr =
                    $"Rôle d'entreprise '{companyRole.Name}' créé avec {request.Permissions?.Count ?? 0} permissions",
                EntityId = companyRole.Id,
            }
        );

        var dto = new CompanyRoleDto(companyRole) { Permissions = request.Permissions };

        return new CreateCompanyRoleResponseModel { Data = dto };
    }
}

public class CreateCompanyRoleResponseModel
{
    public CompanyRoleDto Data { get; set; }
}
