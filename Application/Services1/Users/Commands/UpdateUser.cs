using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Users.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class UpdateUserRequestModel : IRequest<UpdateUserResponseModel>
{
    public string Image { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public bool IsEnabled { get; set; }
    public string Id { get; set; }
}

public class UpdateUserRequestModelValidator : AbstractValidator<UpdateUserRequestModel>
{
    public UpdateUserRequestModelValidator()
    {
        RuleFor(p => p.FullName).Required().Max(50);
        RuleFor(p => p.Email).EmailAddress().Max(50);
    }
}

public class UpdateUserRequestHandler
    : IRequestHandler<UpdateUserRequestModel, UpdateUserResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public UpdateUserRequestHandler(
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

    public async Task<UpdateUserResponseModel> Handle(
        UpdateUserRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var user = await _context.Users.GetByAsync(
            u => u.Id == request.Id,
            cancellationToken: cancellationToken
        );
        var email = request.Email.ToLower();
        user.FullName = request.FullName;
        if (email != user.Email)
        {
            var isEmailTaken = await _context.Users.ActiveAny(u => u.Email == email);
            if (isEmailTaken)
            {
                throw new AlreadyExistsException(nameof(email));
            }
            user.Email = email;
            user.NormalizedEmail = email.ToUpper();
            user.NormalizedUserName = email.ToUpper();
            user.UserName = email;
        }

        if (request.Image != user.Image && request.Image != null)
        {
            user.Image = await _imageService.SaveImageToServer(request.Image, ".png", "users");
        }
        user.IsEnabled = request.IsEnabled;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(), // Assuming this gives the current user
                Feature = AuditLogFeatureType.User, // You may need to add this enum value
                Action = AuditLogType.Update,
                Description = $"User '{user.FullName}' updated with Email '{user.Email}'",
                DescriptionFr =
                    $"Utilisateur '{user.FullName}' mis à jour avec l'email '{user.Email}'",
                EntityId = user.Id,
            }
        );
        return new UpdateUserResponseModel() { Data = new UserDto(user) };
    }
}

public class UpdateUserResponseModel
{
    public UserDto Data { get; set; }
}
