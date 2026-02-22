using Application.Extensions;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Persistence.Context;

namespace Application.Services.AppNotifications.Commands;

public class SendTestNotificationRequestModel : IRequest<SendTestNotificationResponseModel>
{
    public string Id { get; set; }
    public bool IsTopic { get; set; }
}

public class SendTestNotificationRequestModelValidator
    : AbstractValidator<SendTestNotificationRequestModel>
{
    public SendTestNotificationRequestModelValidator()
    {
        RuleFor(x => x.Id).Required();
    }
}

public class SendTestNotificationRequestHandler
    : IRequestHandler<SendTestNotificationRequestModel, SendTestNotificationResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private IBackgroundTaskQueueService _backgroundTaskQueueService;

    private readonly IAlertService _alertService;

    public SendTestNotificationRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService backgroundTaskQueueService,
        IAlertService alertService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _backgroundTaskQueueService = backgroundTaskQueueService;
        _alertService = alertService;
    }

    public async Task<SendTestNotificationResponseModel> Handle(
        SendTestNotificationRequestModel request,
        CancellationToken cancellationToken
    )
    {
        await _alertService.SendNotificationToUser(request.Id, "Test", "Test");
        // _backgroundTaskQueueService.QueueBackgroundWorkItem(
        //     new SendAppNotificationRequestModel() { Id = request.Id }
        // );
        // else
        // {
        //     var fcmIds = _context
        //         .DeviceFcms.Where(x => x.UserId == request.Id)
        //         .Select(x => x.DeviceId)
        //         .ToList();
        //     _backgroundTaskQueueService.QueueBackgroundWorkItem(
        //         new SendAppNotificationRequestModel()
        //         {
        //             FcmIds = fcmIds,
        //             Type = NotificationType.Event,
        //             Body = "Test Body " + DateTime.UtcNow.ToShortTimeString(),
        //             Title = "Test Title",
        //         }
        //     );
        // }

        return new SendTestNotificationResponseModel();
    }
}

public class SendTestNotificationResponseModel
{
    public SendTestNotificationResponseModel() { }
}
