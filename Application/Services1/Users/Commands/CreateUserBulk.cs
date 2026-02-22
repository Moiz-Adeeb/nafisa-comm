using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Emails.Commands;
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

public class CreateUserBulkDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class CreateUserBulkValidator : AbstractValidator<CreateUserBulkDto>
{
    public CreateUserBulkValidator()
    {
        RuleFor(p => p.FullName).Required().Max(50);
        RuleFor(p => p.Email).EmailAddress().Max(50);
    }
}

public class CreateUserBulkRequestModel : IRequest<CreateUserBulkResponseModel>
{
    public string Host { get; set; }
    public List<CreateUserBulkDto> Users { get; set; }
}

public class CreateUserBulkRequestModelValidator : AbstractValidator<CreateUserBulkRequestModel>
{
    public CreateUserBulkRequestModelValidator()
    {
        RuleFor(p => p.Host).Required();
        RuleFor(p => p.Users).Required();
        RuleForEach(p => p.Users).SetValidator(p => new CreateUserBulkValidator());
    }
}

public class CreateUserBulkRequestHandler
    : IRequestHandler<CreateUserBulkRequestModel, CreateUserBulkResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly UserManager<User> _userManager;
    private readonly IBackgroundTaskQueueService _backgroundTaskQueueService;

    public CreateUserBulkRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        UserManager<User> userManager,
        IBackgroundTaskQueueService backgroundTaskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _userManager = userManager;
        _backgroundTaskQueueService = backgroundTaskQueueService;
    }

    public async Task<CreateUserBulkResponseModel> Handle(
        CreateUserBulkRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var emails = _context.Users.GetAllReadOnly().Select(p => p.Email).ToHashSet();
        var sendEmails = new List<SendEmailRequestModel>();
        var auditLogs = new List<CreateAuditLogRequestModel>();
        var list = new List<User>();
        foreach (var createUserBulkDto in request.Users)
        {
            var email = createUserBulkDto.Email.ToLower();
            if (emails.Contains(email))
            {
                continue;
            }
            var user = new User()
            {
                Email = email,
                FullName = createUserBulkDto.FullName,
                NormalizedEmail = email.ToUpper(),
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                IsEnabled = true,
                Image = null,
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
            };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(
                user,
                Guid.NewGuid().ToString()
            );
            list.Add(user);
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            auditLogs.Add(
                new CreateAuditLogRequestModel
                {
                    UserId = _sessionService.GetUserId(), // Assuming this gives the current user
                    Feature = AuditLogFeatureType.User, // You may need to add this enum value
                    Action = AuditLogType.Create,
                    Description = $"User '{user.FullName}' created with Email '{user.Email}'",
                    DescriptionFr =
                        $"Utilisateur '{user.FullName}' créé avec l'email '{user.Email}'",
                    EntityId = user.Id,
                }
            );

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{request.Host}/set-password?token={token}";
            var emailBody =
                $@"<div style=""font-family: Arial, sans-serif;"">
    <p>Bonjour {user.FullName ?? user.Email},</p>
    <p>Nous avons reçu une demande de réinitialisation de votre mot de passe.<br>
    Pour définir un nouveau mot de passe, veuillez cliquer sur le lien ci-dessous :</p>
    
    <p>👉 <a href=""{resetLink}"">Réinitialiser mon mot de passe</a></p>
    
    <p>Ce lien est valide pendant 24 heures.<br>
    Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer ce message.</p>
    
    <p>Merci,<br>L'équipe IT</p>
    
    <hr style=""margin: 20px 0; border: 1px solid #ccc;"">
    
    <p>Hello {user.FullName ?? user.Email},</p>
    <p>We received a request to reset your password.<br>
    To set a new password, please click the link below:</p>
    
    <p>👉 <a href=""{resetLink}"">Reset my password</a></p>
    
    <p>This link will be valid for 24 hours.<br>
    If you did not request this reset, you can safely ignore this message.</p>
    
    <p>Thank you,<br>IT Team</p>
</div>";

            sendEmails.Add(
                new SendEmailRequestModel()
                {
                    UserId = user.Id,
                    IsCheckForEmailAllow = true,
                    Subject = "Password Reset Request",
                    Body = emailBody,
                }
            );
        }
        foreach (CreateAuditLogRequestModel createAuditLogRequestModel in auditLogs)
        {
            _backgroundTaskQueueService.QueueBackgroundWorkItem(createAuditLogRequestModel);
        }
        foreach (var sendEmailRequestModel in sendEmails)
        {
            _backgroundTaskQueueService.QueueBackgroundWorkItem(sendEmailRequestModel);
        }
        return new CreateUserBulkResponseModel()
        {
            Data = list.Select(p => new UserDto()
                {
                    Id = p.Id,
                    Email = p.Email,
                    FullName = p.FullName,
                    CreatedDate = p.CreatedDate,
                    Image = p.Image,
                    IsEnabled = p.IsEnabled,
                })
                .ToList(),
        };
    }
}

public class CreateUserBulkResponseModel
{
    public List<UserDto> Data { get; set; }
}
