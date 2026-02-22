using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Conversations.Models;
using Application.Services.Users.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Persistence.Context;
using Persistence.Extension;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Services.Conversations.Commands;

public class InitializeConversationRequestModel : IRequest<InitializeConversationResponseModel>
{
    public string ChatId { get; set; }
    //public string InitiatorId { get; set; }
}

public class InitializeConversationRequestModelValidator : AbstractValidator<InitializeConversationRequestModel>
{
    public InitializeConversationRequestModelValidator()
    {
        RuleFor(x => x.ChatId).Required();
    }
}

public class InitializeConversationRequestHandler
    : IRequestHandler<InitializeConversationRequestModel, InitializeConversationResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public InitializeConversationRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }
    public async Task<InitializeConversationResponseModel> Handle(
        InitializeConversationRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Conversation, bool>> query = p => true;

        var targetUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ChatId == request.ChatId, cancellationToken);

        if (targetUser == null)
        {
            throw new Exception($"Target user with ChatId {request.ChatId} not found.");
        }

        var TargetUserId = targetUser.Id;
        var TargetUserChatId = targetUser.ChatId;
        var InitiatorId = _sessionService.GetUserId();
        var InitiatorChatId = _sessionService.GetChatId();
        var ConversationId = String.Compare(InitiatorChatId, TargetUserChatId) < 0
            ? $"{InitiatorChatId}_{TargetUserChatId}"
            : $"{TargetUserChatId}_{InitiatorChatId}";

        query = query.AndAlso(p => p.ConversationId.Equals(ConversationId));

        var existCheck = await _context.
            Conversation.GetByWithSelectAsync(query,
            ConversationSelector.Selector(InitiatorId),
            cancellationToken
        );

        if (existCheck != null)
        {
            return new InitializeConversationResponseModel() { Data = existCheck };
        }

        var newConversation = new Conversation()
        {
            ConversationId = ConversationId,
            User1 = TargetUserId,
            User2 = InitiatorId,
            LastMessageTime = DateTimeOffset.UnixEpoch,
            CreatedDate = DateTimeOffset.UtcNow,
        };

        var response = new ConversationDto()
        {
            ConversationId = newConversation.ConversationId,
            User1 = newConversation.User1,
            User2 = newConversation.User2,
            // The Target User is the "Otheruser" from the Initiator's perspective
            OtherUser = new UsersDto
            {
                ChatId = targetUser.ChatId,
                UserName = targetUser.UserName,
                Name = targetUser.Name,
                Status = targetUser.Status,
                LastSeen = targetUser.LastSeen,
            },
            LastMessageTime = newConversation.LastMessageTime,
            CreatedDate = newConversation.CreatedDate,
        };
        await _context.Conversation.AddAsync(newConversation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new InitializeConversationResponseModel() { Data = response };
    }
}

public class InitializeConversationResponseModel
{
    public ConversationDto Data { get; set; }
}
