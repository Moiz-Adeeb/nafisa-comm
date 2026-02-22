using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Application.Services.Users.Models;
using Application.Shared;
using Domain.Entities;
using System.Linq;
using Domain.Enums;
using Common.Extensions;

namespace Application.Services.Conversations.Models;

public class ConversationDetailDto : ConversationDto { }

public class ConversationDto
{
    public string ConversationId { get; set; }
    public string User1 { get; set; }
    public string User2 { get; set; } 
    public UsersDto OtherUser { get; set; }
    public DateTimeOffset LastMessageTime { get; set; }
    public string LastMessageSnippet { get; set; }
    public int unreadCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public ConversationDto() { }

    public ConversationDto(Conversation conversation, User user)
    {
        ConversationId = conversation.ConversationId;
        User1 = conversation.User1;
        User2 = conversation.User2;
        OtherUser = new UsersDto
        {
            ChatId = user.ChatId,
            Name = user.Name,
            UserName = user.UserName,
            Status = user.Status,
            LastSeen = user.LastSeen,
        };
        LastMessageTime = conversation.LastMessageTime;
        unreadCount = 0;
        CreatedDate = conversation.CreatedDate;
    }
}

public class ConversationSelector
{
    public static Expression<Func<Conversation, ConversationDto>> Selector(string userId)
    {
        return p => new ConversationDto()
        {
            ConversationId = p.ConversationId,
            User1 = p.User1Navigation.ChatId,
            User2 = p.User2Navigation.ChatId,
            OtherUser = new UsersDto
            {
                ChatId = (p.User1 == userId ? p.User2Navigation : p.User1Navigation).ChatId,
                UserName = (p.User1 == userId ? p.User2Navigation : p.User1Navigation).UserName,
                Name = (p.User1 == userId ? p.User2Navigation : p.User1Navigation).Name,
                Status = (p.User1 == userId ? p.User2Navigation : p.User1Navigation).Status,
                LastSeen = (p.User1 == userId ? p.User2Navigation : p.User1Navigation).LastSeen,
            },
            LastMessageTime = p.LastMessageTime,
            LastMessageSnippet = p.Messages
                .OrderByDescending(m => m.SentTime)
                .Select(m => m.MessageContent)
                .FirstOrDefault(),
            unreadCount = p.Messages
                .Count(m => 
                    m.ReceiverId == userId && 
                    m.Status != MessageStatus.Read
                ),
            CreatedDate = p.CreatedDate,
        };
    }
    public static readonly Expression<Func<Conversation, ConversationDetailDto>> SelectorDetail =
        p => new ConversationDetailDto()
        {
            ConversationId = p.ConversationId,
            User1 = p.User1,
            User2 = p.User2,
            LastMessageTime = p.LastMessageTime,
            LastMessageSnippet = p.Messages.OrderByDescending(m => m.SentTime)
                .Select(m => m.MessageContent)
                .FirstOrDefault(),
            CreatedDate = p.CreatedDate,
        };

    public static readonly Expression<Func<Conversation, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Name = p.ConversationId, Id = p.Id };
}
