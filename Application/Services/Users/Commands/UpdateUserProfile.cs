using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Constant;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class UpdateUserProfileRequestModel : IRequest<UpdateUserProfileResponseModel>
{
    public string Name { get; set; }
    public string Image { get; set; }
    public bool IsAllowEmail { get; set; }
    public bool IsAllowNotification { get; set; }
}

public class UpdateUserProfileRequestModelValidator
    : AbstractValidator<UpdateUserProfileRequestModel>
{
    public UpdateUserProfileRequestModelValidator()
    {
        RuleFor(p => p.Name).Required().Max(52);
    }
}

public class UpdateUserProfileRequestHandler
    : IRequestHandler<UpdateUserProfileRequestModel, UpdateUserProfileResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IImageService _imageService;

    public UpdateUserProfileRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IImageService imageService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _imageService = imageService;
    }

    public async Task<UpdateUserProfileResponseModel> Handle(
        UpdateUserProfileRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var user = await _context.Users.GetByAsync(
            p => p.Id == userId,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            throw new NotFoundException(nameof(user));
        }
        user.Name = request.Name;
        if (request.Image == null)
        {
            user.Image = null;
        }
        else if (request.Image != user.Image)
        {
            user.Image = await _imageService.SaveImageToServer(request.Image, ".png", "users");
        }
        var settings = _context.UserSettings.GetByReadOnly(p =>
            p.UserId == userId && p.Key == SettingKeyConstant.AllowEmailSetting
        );
        if (settings != null)
        {
            settings.Value = request.IsAllowEmail.ToString();
            _context.UserSettings.Update(settings);
        }
        var notificationSettings = _context.UserSettings.GetByReadOnly(p =>
            p.UserId == userId && p.Key == SettingKeyConstant.AllowNotificationSetting
        );
        if (notificationSettings != null)
        {
            notificationSettings.Value = request.IsAllowNotification.ToString();
            _context.UserSettings.Update(notificationSettings);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return new UpdateUserProfileResponseModel()
        {
            FullName = request.Name,
            Image = request.Image,
            IsAllowEmail = request.IsAllowEmail,
            IsAllowNotification = request.IsAllowNotification,
        };
    }
}

public class UpdateUserProfileResponseModel
{
    public string FullName { get; set; }
    public string Image { get; set; }
    public bool IsAllowEmail { get; set; }
    public bool IsAllowNotification { get; set; }
}