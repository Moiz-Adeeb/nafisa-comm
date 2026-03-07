using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class DeleteUserRequestModel : IRequest<DeleteUserResponseModel>
{
    public string Id { get; set; }
}

public class DeleteUserRequestModelValidator : AbstractValidator<DeleteUserRequestModel>
{
    public DeleteUserRequestModelValidator()
    {
        RuleFor(x => x.Id).Required();
    }
}

public class DeleteUserRequestHandler
    : IRequestHandler<DeleteUserRequestModel, DeleteUserResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteUserRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteUserResponseModel> Handle(
        DeleteUserRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var user = await _context.Users.GetByAsync(
            u => u.Id == request.Id,
            cancellationToken: cancellationToken
        );
        
        if (user == null) throw new NotFoundException(nameof(user));
        user.IsEnabled = false;
        user.IsDeleted = true;
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new DeleteUserResponseModel();
    }
}

public class DeleteUserResponseModel { }