using Application.Exceptions;
using Application.Interfaces;
using Application.Services.Users.Models;
using Domain.Constant;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Queries;

public class GetUserProfileRequestModel : IRequest<GetUserProfileResponseModel> { }

public class GetUserProfileRequestModelValidator : AbstractValidator<GetUserProfileRequestModel>
{
    public GetUserProfileRequestModelValidator() { }
}

public class GetUserProfileRequestHandler
    : IRequestHandler<GetUserProfileRequestModel, GetUserProfileResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetUserProfileRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetUserProfileResponseModel> Handle(
        GetUserProfileRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var data = await _context.Users.GetByWithSelectAsync(
            p => p.Id == userId,
            UserSelector.Selector,
            cancellationToken: cancellationToken
        );
        if (data == null)
        {
            throw new NotFoundException(nameof(User));
        }
        var emailSetting = await _context.UserSettings.GetByWithSelectAsync(
            p => p.UserId == userId && p.Key == SettingKeyConstant.AllowEmailSetting,
            p => p.Value,
            cancellationToken: cancellationToken
        );
        var notificationSetting = await _context.UserSettings.GetByWithSelectAsync(
            p => p.UserId == userId && p.Key == SettingKeyConstant.AllowNotificationSetting,
            p => p.Value,
            cancellationToken: cancellationToken
        );
        data.IsAllowEmail = emailSetting == true.ToString();
        data.IsAllowNotification = notificationSetting == true.ToString();
        return new GetUserProfileResponseModel() { Data = data };
    }
}

public class GetUserProfileResponseModel
{
    public UserDto Data { get; set; }
}
