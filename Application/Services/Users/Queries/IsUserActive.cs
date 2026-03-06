using Application.Interfaces;
using Domain.Constant;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Queries;

public class IsUserActiveRequestModel : IRequest<IsUserActiveResponseModel> { }

public class IsUserActiveRequestModelValidator : AbstractValidator<IsUserActiveRequestModel>
{
    public IsUserActiveRequestModelValidator() { }
}

public class IsUserActiveRequestHandler : IRequestHandler<IsUserActiveRequestModel, IsUserActiveResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public IsUserActiveRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<IsUserActiveResponseModel> Handle(
        IsUserActiveRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var isActive = await _context.Users.ActiveAny(
            p => p.IsEnabled && p.Id == userId,
            token: cancellationToken
        );
        return new IsUserActiveResponseModel() { IsActive = isActive };
    }
}

public class IsUserActiveResponseModel
{
    public bool IsActive { get; set; }
}