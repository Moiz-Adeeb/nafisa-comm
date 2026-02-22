using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.StaffAdmin.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.StaffAdmin.Commands;

public class CreateStaffAdminRequestModel : IRequest<CreateStaffAdminResponseModel>
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string Image { get; set; }
    public string AdminRoleId { get; set; } // Optional custom admin role
}

public class CreateStaffAdminRequestModelValidator : AbstractValidator<CreateStaffAdminRequestModel>
{
    public CreateStaffAdminRequestModelValidator()
    {
        RuleFor(p => p.FullName).Required().Max(100);
        RuleFor(p => p.AdminRoleId).Required().Max(100);
        RuleFor(p => p.Email).Required().EmailAddress().Max(100);
        RuleFor(p => p.PhoneNumber).Max(20);
        RuleFor(p => p.Password).Required().Password().Max(100);
        RuleFor(p => p.ConfirmPassword).Required().Matches(p => p.Password);
    }
}

public class CreateStaffAdminRequestHandler
    : IRequestHandler<CreateStaffAdminRequestModel, CreateStaffAdminResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public CreateStaffAdminRequestHandler(
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

    public async Task<CreateStaffAdminResponseModel> Handle(
        CreateStaffAdminRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower().Trim();
        // Check if user already exists
        var isExist = await _context.Users.ActiveAny(u => u.Email == email);
        if (isExist)
        {
            throw new AlreadyExistsException($"User with email '{email}' already exists");
        }

        var adminRoleExists = await _context.AdminRoles.AnyAsync(
            r => r.Id == request.AdminRoleId && !r.IsDeleted && r.IsActive,
            cancellationToken
        );

        if (!adminRoleExists)
        {
            throw new NotFoundException("Admin role not found or inactive");
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
            AdminRoleId = request.AdminRoleId,
            Image =
                request.Image != null
                    ? await _imageService.SaveImageToServer(request.Image, ".png", "staff-admin")
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
            UserRoles = new List<UserRole> { new UserRole { RoleId = request.AdminRoleId } },
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
                Description = $"Admin staff '{user.FullName}' created with email '{user.Email}'",
                DescriptionFr =
                    $"Personnel administrateur '{user.FullName}' créé avec l'email '{user.Email}'",
                EntityId = user.Id,
            }
        );

        var dto = new StaffAdminDto(user) { RoleName = RoleNames.Administrator };

        return new CreateStaffAdminResponseModel { Data = dto };
    }
}

public class CreateStaffAdminResponseModel
{
    public StaffAdminDto Data { get; set; }
}
