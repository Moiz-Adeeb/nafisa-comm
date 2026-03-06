using Application.Extensions;
using Application.Interfaces;
using Common.Extensions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Emails.Commands;

public class SendEmailRequestModel : IRequest<SendEmailResponseModel>
{
    public string UserId { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsCheckForEmailAllow { get; set; }
}

public class SendEmailRequestModelValidator : AbstractValidator<SendEmailRequestModel>
{
    public SendEmailRequestModelValidator()
    {
        RuleFor(p => p.Subject).Required();
        RuleFor(p => p.Body).Required();
    }
}

public class SendEmailRequestHandler
    : IRequestHandler<SendEmailRequestModel, SendEmailResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly ISmtpService _smtpService;
    private readonly ILogger<SendEmailRequestHandler> _logger;

    public SendEmailRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        ISmtpService smtpService,
        ILogger<SendEmailRequestHandler> logger
    )
    {
        _context = context;
        _sessionService = sessionService;
        _smtpService = smtpService;
        _logger = logger;
    }

    public async Task<SendEmailResponseModel> Handle(
        SendEmailRequestModel request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("Send Email request");
        _logger.LogDebug(JsonConvert.SerializeObject(request));
        User user = null;
        if (request.UserId.IsNotNullOrWhiteSpace())
        {
            if (request.IsCheckForEmailAllow)
            {
                user = await _context.Users.GetByWithSelectAsync(
                    p => p.Id == request.UserId,
                    p => new User() { Email = p.Email },
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                user = await _context.Users.GetByWithSelectAsync(
                    p => p.Id == request.UserId,
                    p => new User() { Email = p.Email },
                    cancellationToken: cancellationToken
                );
            }
        }

        try
        {
            if (user != null)
            {
                if (request.IsCheckForEmailAllow)
                {
                    await _smtpService.SendEmailAsync(user.Email, request.Subject, request.Body);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
        return new SendEmailResponseModel();
    }
}

public class SendEmailResponseModel { }