using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.CompanyStaff.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyStaff.Commands;

public class CreateCompanyStaffRequestModel : IRequest<CreateCompanyStaffResponseModel>
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string Image { get; set; }
    public string CompanyRoleId { get; set; } // Optional custom company role
    public string BranchId { get; set; } // If entity supports branches
    public decimal Salary { get; set; } // If entity supports salary
}

public class CreateCompanyStaffRequestModelValidator
    : AbstractValidator<CreateCompanyStaffRequestModel>
{
    public CreateCompanyStaffRequestModelValidator()
    {
        RuleFor(p => p.FullName).Required().Max(100);
        RuleFor(p => p.Email).Required().EmailAddress().Max(100);
        RuleFor(p => p.PhoneNumber).Max(20);
        RuleFor(p => p.Salary).Required();
        RuleFor(p => p.BranchId).Required();
        RuleFor(p => p.Password).Required().Pin();
        RuleFor(p => p.ConfirmPassword).Required().Matches(p => p.Password);
    }
}

public class CreateCompanyStaffRequestHandler
    : IRequestHandler<CreateCompanyStaffRequestModel, CreateCompanyStaffResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public CreateCompanyStaffRequestHandler(
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

    public async Task<CreateCompanyStaffResponseModel> Handle(
        CreateCompanyStaffRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower().Trim();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can create company staff."
            );
        }

        // Check if user already exists
        var isExist = await _context.Users.ActiveAny(u => u.Email == email);
        if (isExist)
        {
            throw new AlreadyExistsException($"User with email '{email}' already exists");
        }

        var companyRole = await _context.CompanyRoles.GetByAsync(
            r => r.Id == request.CompanyRoleId && r.CompanyId == companyId && r.IsActive,
            cancellationToken: cancellationToken
        );

        if (companyRole == null)
        {
            throw new NotFoundException("Company role not found or inactive");
        }
        // Create user
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsEnabled = true,
            // CompanyId = companyId, // If entity supports CompanyId
            // BranchId = request.BranchId, // If entity supports BranchId
            // Salary = request.Salary, // If entity supports Salary
            Image =
                request.Image != null
                    ? await _imageService.SaveImageToServer(request.Image, ".png", "company-staff")
                    : null,
            UserSettings = new List<UserSetting>
            {
                new UserSetting
                {
                    Key = SettingKeyConstant.AllowEmailSetting,
                    Value = true.ToString(),
                },
                new UserSetting
                {
                    Key = SettingKeyConstant.AllowNotificationSetting,
                    Value = true.ToString(),
                },
            },
            CompanyStaff = new global::Domain.Entities.CompanyStaff()
            {
                CompanyRoleId = request.CompanyRoleId,
                BranchId = request.BranchId,
                Salary = request.Salary,
            },
        };

        // Hash password
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.User,
                Action = AuditLogType.Create,
                Description = $"Company staff '{user.FullName}' created with email '{user.Email}'",
                DescriptionFr =
                    $"Personnel de l'entreprise '{user.FullName}' créé avec l'email '{user.Email}'",
                EntityId = user.Id,
            }
        );

        var dto = new CompanyStaffDto(user) { RoleName = companyRole.Name };

        return new CreateCompanyStaffResponseModel { Data = dto };
    }
}

public class CreateCompanyStaffResponseModel
{
    public CompanyStaffDto Data { get; set; }
}
