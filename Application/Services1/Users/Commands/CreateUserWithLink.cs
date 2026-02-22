using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Emails.Commands;
using Application.Services.Users.Models;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class CreateUserWithLinkRequestModel : IRequest<CreateUserWithLinkResponseModel>
{
    public string Host { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}

public class CreateUserWithLinkRequestModelValidator
    : AbstractValidator<CreateUserWithLinkRequestModel>
{
    public CreateUserWithLinkRequestModelValidator()
    {
        RuleFor(p => p.FullName).Required().Max(50);
        RuleFor(p => p.Email).EmailAddress().Max(50);
    }
}

public class CreateUserWithLinkRequestHandler
    : IRequestHandler<CreateUserWithLinkRequestModel, CreateUserWithLinkResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;
    private readonly UserManager<User> _userManager;

    public CreateUserWithLinkRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IImageService imageService,
        UserManager<User> userManager
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _imageService = imageService;
        _userManager = userManager;
    }

    public async Task<CreateUserWithLinkResponseModel> Handle(
        CreateUserWithLinkRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var token = "";
        var email = request.Email.ToLower();
        var existingUser = await _context.Users.GetByReadOnlyAsync(
            u => u.Email == email,
            cancellationToken: cancellationToken
        );
        var resetLink = "";
        var emailBody = "";
        if (existingUser != null)
        {
            token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            resetLink = $"{request.Host}/set-password?token={token}";
            emailBody =
                $@"<div style=""font-family: Arial, sans-serif;"">
    <p>Bonjour {existingUser.FullName ?? existingUser.Email},</p>
    <p>Nous avons reçu une demande de réinitialisation de votre mot de passe.<br>
    Pour définir un nouveau mot de passe, veuillez cliquer sur le lien ci-dessous :</p>
    
    <p>👉 <a href=""{resetLink}"">Réinitialiser mon mot de passe</a></p>
    
    <p>Ce lien est valide pendant 24 heures.<br>
    Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer ce message.</p>
    
    <p>Merci,<br>L'équipe IT</p>
    
    <hr style=""margin: 20px 0; border: 1px solid #ccc;"">
    
    <p>Hello {existingUser.FullName ?? existingUser.Email},</p>
    <p>We received a request to reset your password.<br>
    To set a new password, please click the link below:</p>
    
    <p>👉 <a href=""{resetLink}"">Reset my password</a></p>
    
    <p>This link will be valid for 24 hours.<br>
    If you did not request this reset, you can safely ignore this message.</p>
    
    <p>Thank you,<br>IT Team</p>
</div>";

            _taskQueueService.QueueBackgroundWorkItem(
                new SendEmailRequestModel()
                {
                    UserId = existingUser.Id,
                    IsCheckForEmailAllow = true,
                    Subject = "Password Reset Request",
                    Body = emailBody,
                }
            );
            return new CreateUserWithLinkResponseModel() { Data = new UserDto(existingUser) };
        }

        var user = new User()
        {
            Email = email,
            FullName = request.FullName,
            NormalizedEmail = email.ToUpper(),
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            IsEnabled = true,
            Image = null,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(
            user,
            Guid.NewGuid().ToString()
        );
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
        token = await _userManager.GeneratePasswordResetTokenAsync(user);
        resetLink = $"{request.Host}/set-password?token={token}";
        emailBody =
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

        _taskQueueService.QueueBackgroundWorkItem(
            new SendEmailRequestModel()
            {
                UserId = user.Id,
                IsCheckForEmailAllow = true,
                Subject = "Password Reset Request",
                Body = emailBody,
            }
        );
        return new CreateUserWithLinkResponseModel() { Data = new UserDto(user) };
    }
}

public class CreateUserWithLinkResponseModel
{
    public UserDto Data { get; set; }
}
