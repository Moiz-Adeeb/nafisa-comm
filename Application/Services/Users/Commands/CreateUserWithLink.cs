using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
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
                    <p>Hello {existingUser.Name ?? existingUser.Email},</p>
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
            Name = request.FullName,
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
        token = await _userManager.GeneratePasswordResetTokenAsync(user);
        resetLink = $"{request.Host}/set-password?token={token}";
        emailBody =
            $@"<div style=""font-family: Arial, sans-serif;"">
                <p>Hello {user.Name ?? user.Email},</p>
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