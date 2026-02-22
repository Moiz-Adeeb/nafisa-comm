using Application.Extensions;
using Application.Interfaces;
using Application.Services.Message.Models;
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

namespace Application.Services.Message.Queries;

public class GetMessageRequestModel : GetPagedRequest<GetMessageResponseModel>
{
    public string ConversationId { get; set; }
}

public class GetMessageRequestModelValidator : PageRequestValidator<GetMessageRequestModel>
{
    public GetMessageRequestModelValidator()
    {
        RuleFor(x => x.ConversationId).Required();
    }
}

public class GetMessageRequestHandler : IRequestHandler<GetMessageRequestModel, GetMessageResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetMessageRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetMessageResponseModel> Handle(
        GetMessageRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Messages, bool>> query = p => true;
        var userId = _sessionService.GetUserId();
        var conversation = await _context.Conversation
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.ConversationId == request.ConversationId,
                cancellationToken
            );

        query = query.AndAlso(p => p.ConversationId.Equals(conversation.Id));

        var list = await _context
            .Messages.GetManyReadOnly(query, request)
            .Select(MessagesSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Messages.CountAsync(query, cancellationToken: cancellationToken);
        return new GetMessageResponseModel() { Data = list, Count = count };
    }
}

public class GetMessageResponseModel
{
    public List<MessagesDto> Data { get; set; }
    public int Count { get; set; }
}
