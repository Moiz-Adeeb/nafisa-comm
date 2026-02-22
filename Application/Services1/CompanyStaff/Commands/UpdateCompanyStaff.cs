using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.CompanyStaff.Models;
using Domain.Constant;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyStaff.Commands;

public class UpdateCompanyStaffRequestModel : IRequest<UpdateCompanyStaffResponseModel>
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Image { get; set; }
    public string CompanyRoleId { get; set; } // Optional custom company role
    public bool IsEnabled { get; set; }
    public string BranchId { get; set; } // If entity supports branches
    public decimal Salary { get; set; } // If entity supports salary
}

public class UpdateCompanyStaffRequestModelValidator
    : AbstractValidator<UpdateCompanyStaffRequestModel>
{
    public UpdateCompanyStaffRequestModelValidator()
    {
        RuleFor(p => p.Salary).Required();
        RuleFor(p => p.BranchId).Required();
        RuleFor(p => p.FullName).Required().Max(100);
        RuleFor(p => p.Email).Required().EmailAddress().Max(100);
        RuleFor(p => p.PhoneNumber).Max(20);
    }
}

public class UpdateCompanyStaffRequestHandler
    : IRequestHandler<UpdateCompanyStaffRequestModel, UpdateCompanyStaffResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public UpdateCompanyStaffRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IImageService imageService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _imageService = imageService;
    }

    public async Task<UpdateCompanyStaffResponseModel> Handle(
        UpdateCompanyStaffRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower().Trim();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can update company staff."
            );
        }

        // Get existing user
        var user = await _context.Users.GetByAsync(
            p => p.Id == request.Id && p.CompanyStaff.CompanyId == companyId,
            p => p.Include(x => x.CompanyStaff).Include(x => x.UserRoles).ThenInclude(x => x.Role)
        );

        if (user == null)
        {
            throw new NotFoundException("Company staff not found");
        }

        // Verify user is company staff (has CompanyAdmin or Employee role)
        var isCompanyStaff = user.UserRoles.Any(ur => ur.Role.Name == RoleNames.Employee);

        if (!isCompanyStaff)
        {
            throw new BadRequestException("User is not a company staff member");
        }

        // Tenant validation: verify user belongs to same company
        // if (user.CompanyId != companyId) // If entity supports CompanyId
        // {
        //     throw new BadRequestException("Cannot update staff from a different company");
        // }

        // Check if email already exists (excluding current user)
        var emailExists = await _context.Users.AnyAsync(
            u => u.Email == email && u.Id != request.Id && !u.IsDeleted,
            cancellationToken
        );

        if (emailExists)
        {
            throw new AlreadyExistsException($"User with email '{email}' already exists");
        }

        var companyRole = await _context.CompanyRoles.FirstOrDefaultAsync(
            r => r.Id == request.CompanyRoleId && !r.IsDeleted && r.IsActive,
            cancellationToken
        );

        if (companyRole == null)
        {
            throw new NotFoundException("Company role not found or inactive");
        }

        // Validate tenant ownership
        if (companyRole.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot assign company role from a different company");
        }

        // Update user properties
        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.NormalizedEmail = email.ToUpper();
        user.UserName = email;
        user.NormalizedUserName = email.ToUpper();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.CompanyStaff.CompanyRoleId = request.CompanyRoleId;
        user.IsEnabled = request.IsEnabled;
        user.CompanyStaff.BranchId = request.BranchId; // If entity supports BranchId
        user.CompanyStaff.Salary = request.Salary; // If entity supports Salary

        // Update image if provided
        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            if (user.Image != request.Image)
            {
                user.Image = await _imageService.SaveImageToServer(
                    request.Image,
                    ".png",
                    "company-staff"
                );
            }
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.User,
                Action = AuditLogType.Update,
                Description = $"Company staff '{user.FullName}' updated",
                DescriptionFr = $"Personnel de l'entreprise '{user.FullName}' mis à jour",
                EntityId = user.Id,
            }
        );

        var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name;
        var dto = new CompanyStaffDto(user) { RoleName = roleName };

        return new UpdateCompanyStaffResponseModel { Data = dto };
    }
}

public class UpdateCompanyStaffResponseModel
{
    public CompanyStaffDto Data { get; set; }
}
