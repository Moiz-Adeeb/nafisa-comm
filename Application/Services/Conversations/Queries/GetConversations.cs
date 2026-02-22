using Application.Extensions;
using Application.Interfaces;
using Application.Services.Conversations.Models;
using Application.Services.Users.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Constant;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;

namespace Application.Services.Conversations.Queries;

public class GetConversationsRequestModel : GetPagedRequest<GetConversationsResponseModel>
{
    //public string ChatId { get; set; }
}

public class GetConversationsRequestModelValidator : PageRequestValidator<GetConversationsRequestModel>
{
    public GetConversationsRequestModelValidator()
    {
        
    }
}

public class GetConversationsRequestHandler : IRequestHandler<GetConversationsRequestModel, GetConversationsResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetConversationsRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetConversationsResponseModel> Handle(
        GetConversationsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Conversation, bool>> query = p => true;
        var userId = _sessionService.GetUserId();

        query = query.AndAlso(p => p.User1.Equals(userId) || p.User2.Equals(userId));

        var list = await _context.Conversation
            .GetManyReadOnly(query, request)
            .Select(ConversationSelector.Selector(userId))
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Conversation.CountAsync(query, cancellationToken: cancellationToken);

        return new GetConversationsResponseModel() { Data = list, Count = count};
    }
}

public class GetConversationsResponseModel
{
    public List<ConversationDto> Data { get; set; }
    public int Count { get; set; }
}
