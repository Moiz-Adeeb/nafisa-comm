using Application.Exceptions;
using Application.Interfaces;
using Application.Services.Users.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Queries;

public class GetUserByIdRequestModel : IRequest<GetUserByIdResponseModel>
{
    public string ChatId { get; set; }
}

public class GetUserByIdRequestModelValidator : AbstractValidator<GetUserByIdRequestModel>
{
    public GetUserByIdRequestModelValidator() { }
}

public class GetUserByIdRequestHandler
    : IRequestHandler<GetUserByIdRequestModel, GetUserByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetUserByIdRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetUserByIdResponseModel> Handle(
        GetUserByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Users.GetByWithSelectAsync(
            p => p.ChatId == request.ChatId,
            UsersSelector.SelectorDetail,
            cancellationToken: cancellationToken
        );
        if (data == null)
        {
            throw new NotFoundException(nameof(User));
        }
        return new GetUserByIdResponseModel() { Data = data };
    }
}

public class GetUserByIdResponseModel
{
    public UsersDetailDto Data { get; set; }
}
