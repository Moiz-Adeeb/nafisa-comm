using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Emails.Commands;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class GenerateResetPasswordTokenRequestModel
    : IRequest<GenerateResetPasswordTokenResponseModel>
{
    public string Host { get; set; }
    public string Email { get; set; }
}

public class GenerateResetPasswordTokenRequestModelValidator
    : AbstractValidator<GenerateResetPasswordTokenRequestModel>
{
    public GenerateResetPasswordTokenRequestModelValidator()
    {
        RuleFor(x => x.Email).EmailAddress().Max(50);
    }
}

public class GenerateResetPasswordTokenRequestHandler
    : IRequestHandler<
        GenerateResetPasswordTokenRequestModel,
        GenerateResetPasswordTokenResponseModel
    >
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly UserManager<User> _userManager;
    private readonly ISmtpService _smtpService;
    private readonly IBackgroundTaskQueueService _backgroundTaskQueueService;

    public GenerateResetPasswordTokenRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        UserManager<User> userManager,
        ISmtpService smtpService,
        IBackgroundTaskQueueService backgroundTaskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _userManager = userManager;
        _smtpService = smtpService;
        _backgroundTaskQueueService = backgroundTaskQueueService;
    }

    public async Task<GenerateResetPasswordTokenResponseModel> Handle(
        GenerateResetPasswordTokenRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var user = await _context.Users.GetByAsync(
            u => u.Email == request.Email,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            throw new NotFoundException(nameof(User.Email));
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{request.Host}/reset-password?token={token}";
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

        _backgroundTaskQueueService.QueueBackgroundWorkItem(
            new SendEmailRequestModel()
            {
                UserId = user.Id,
                IsCheckForEmailAllow = true,
                Subject = "Password Reset Request",
                Body = emailBody,
            }
        );
        return new GenerateResetPasswordTokenResponseModel()
        {
            Url = request.Host + "/reset-password?token=" + token + "&email=" + request.Email,
        };
    }
}

public class GenerateResetPasswordTokenResponseModel
{
    public string Url { get; set; }
}
