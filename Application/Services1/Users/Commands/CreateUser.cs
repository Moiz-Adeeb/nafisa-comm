using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Users.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class CreateUserRequestModel : IRequest<CreateUserResponseModel>
{
    public string Image { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateUserRequestModelValidator : AbstractValidator<CreateUserRequestModel>
{
    public CreateUserRequestModelValidator()
    {
        RuleFor(p => p.Role).MustBeOneOf(RoleNames.AllRoles);
        RuleFor(p => p.FullName).Required().Max(50);
        RuleFor(p => p.Email).EmailAddress().Max(50);
        RuleFor(p => p.Password).Password().Max(50);
        RuleFor(p => p.ConfirmPassword).Matches(p => p.Password);
    }
}

public class CreateUserRequestHandler
    : IRequestHandler<CreateUserRequestModel, CreateUserResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public CreateUserRequestHandler(
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

    public async Task<CreateUserResponseModel> Handle(
        CreateUserRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower();
        var isExist = await _context.Users.ActiveAny(u => u.Email == email);
        if (isExist)
        {
            throw new AlreadyExistsException(nameof(email));
        }

        var roleId = await _context.Roles.GetByWithSelectAsync(
            p => p.Name == request.Role,
            p => p.Id,
            cancellationToken: cancellationToken
        );
        var user = new User()
        {
            Email = email,
            FullName = request.FullName,
            NormalizedEmail = email.ToUpper(),
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            IsEnabled = request.IsEnabled,
            Image =
                request.Image != null
                    ? await _imageService.SaveImageToServer(request.Image, ".png", "users")
                    : null,
            UserSettings = new List<UserSetting>()
            {
                new UserSetting()
                {
                    Key = SettingKeyConstant.AllowEmailSetting,
                    Value = true.ToString(),
                },
                new UserSetting()
                {
                    Key = SettingKeyConstant.AllowNotificationSetting,
                    Value = true.ToString(),
                },
            },
            UserRoles = new List<UserRole>() { new UserRole() { RoleId = roleId } },
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(), // Assuming this gives the current user
                Feature = AuditLogFeatureType.User, // You may need to add this enum value
                Action = AuditLogType.Create,
                Description = $"User '{user.FullName}' created with Email '{user.Email}'",
                DescriptionFr = $"Utilisateur '{user.FullName}' créé avec l'email '{user.Email}'",
                EntityId = user.Id,
            }
        );
        return new CreateUserResponseModel() { Data = new UserDto(user) };
    }
}

public class CreateUserResponseModel
{
    public UserDto Data { get; set; }
}
