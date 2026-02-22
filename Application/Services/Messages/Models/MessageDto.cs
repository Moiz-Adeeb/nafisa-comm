using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Message.Models;
using Application.Shared;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Message.Models;

public class MessagesDetailDto : MessagesDto { }

public class MessagesDto
{
    public string Id { get; set; }
    public string ConversationId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string MessageContent { get; set; }
    public MessageStatus Status { get; set; }
    public DateTimeOffset SentTime { get; set; }
    public DateTimeOffset? DeliveredTime { get; set; }
    public DateTimeOffset? ReadTime { get; set; }

    public MessagesDto() { }

    public MessagesDto(Messages messsage, string senderChatId, string receiverChatId)
    {
        Id = messsage.Id;
        ConversationId = messsage.ConversationId;
        SenderId = senderChatId;
        ReceiverId = receiverChatId;
        MessageContent = messsage.MessageContent;
        Status = messsage.Status;
        SentTime = messsage.SentTime;;
    }
}

public class MessagesSelector
{
    public static Expression<Func<Messages, MessagesDto>> Selector = p => new MessagesDto()
        {
            Id = p.Id,
            ConversationId = p.Conversation.ConversationId,
            SenderId = p.Sender.ChatId,
            ReceiverId = p.Receiver.ChatId,
            MessageContent = p.MessageContent,
            Status = p.Status,
            SentTime = p.SentTime,
            DeliveredTime = p.DeliveredTime,
            ReadTime = p.ReadTime,
        };
    
    public static readonly Expression<Func<Messages, MessagesDetailDto>> SelectorDetail =
        p => new MessagesDetailDto()
        {
            Id = p.Id,
            ConversationId = p.Conversation.Id,
            SenderId = p.Sender.ChatId,
            ReceiverId = p.Receiver.ChatId,
            MessageContent = p.MessageContent,
            Status = p.Status,
            SentTime = p.SentTime,
            DeliveredTime = p.DeliveredTime,
            ReadTime = p.ReadTime,
        };

    public static readonly Expression<Func<Messages, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Name = p.Conversation.Id, Id = p.Id };
}
