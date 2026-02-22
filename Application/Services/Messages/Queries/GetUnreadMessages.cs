using Application.Extensions;
using Application.Interfaces;
using Application.Services.Message.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;
using System.Linq.Expressions;
using FluentValidation;
using System.Security.Cryptography.X509Certificates;

namespace Application.Services.Message.Queries;

public class GetUnreadMessageRequestModel : IRequest<GetUnreadMessageResponseModel>
{

}

public class GetUnreadMessageRequestModelValidator : AbstractValidator<GetUnreadMessageRequestModel>
{
    public GetUnreadMessageRequestModelValidator()
    {

    }
}

public class GetUnreadMessageRequestHandler : IRequestHandler<GetUnreadMessageRequestModel, GetUnreadMessageResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetUnreadMessageRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetUnreadMessageResponseModel> Handle(
        GetUnreadMessageRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Messages, bool>> query = p => true;
        var userId = _sessionService.GetUserId();

        query = query.AndAlso(p => p.ReceiverId.Equals(userId))
            .AndAlso(p => p.Status < MessageStatus.Read);

        var count = await _context.Messages.CountAsync(query, cancellationToken: cancellationToken);
        return new GetUnreadMessageResponseModel() { Count = count };
    }
}

public class GetUnreadMessageResponseModel
{
    public int Count { get; set; }
}
