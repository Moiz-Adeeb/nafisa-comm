using Application.Interfaces;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Collections.Concurrent;
using System.Threading;
using WebApi.Extension;
using Application.Services.Message.Models;
using Application.Services.Conversations.Models;
using Common.Extensions;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace WebApi.Hubs;

[Authorize]
public class ChatHub : Hub
{

    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    //Dictionary To Maintain Active Connections Based on UserId
    private static readonly ConcurrentDictionary<string, List<string>> Connections = new();
    //Dictionary To Maintain Active Conversations Per Connection
    private static readonly ConcurrentDictionary<string, string> ActiveConversation = new();

    //Establishing WebSocket Connection
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.GetUserId();
        var chatId = Context.User.GetChatId();
        var connectionId = Context.ConnectionId;

        var connections = Connections.GetOrAdd(userId, _ => new List<string>());

        lock (connections)
        {
            //Add the new connection ID to the user's list of connections
            if (!connections.Contains(connectionId)) connections.Add(connectionId);
        }


        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            Context.User.GetUserId(),
            Context.ConnectionAborted
        );

        var status = new
        {
            ChatId = chatId,
            isOnline = true,
        };

        await Clients.All.SendAsync("IsOnline", new { ChatId = chatId, IsOnline = true });

        var pendingMessages = await _context.Messages
            .Where(m =>
                m.ReceiverId == userId &&
                m.Status == MessageStatus.Sent
            )
            .ToListAsync();

        if (pendingMessages.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var message in pendingMessages)
            {
                message.Status = MessageStatus.Delivered;
                message.DeliveredTime = now;
            }

            await _context.SaveChangesAsync();

            // Notify senders in batches
            foreach (var senderGroup in pendingMessages.GroupBy(m => m.SenderId))
            {
                await Clients.User(senderGroup.Key).SendAsync(
                    "MarkAsReceivedBatch",
                    senderGroup.Select(m => new
                    {
                        m.Id,
                        m.DeliveredTime
                    })
                );
            }
        }

        await base.OnConnectedAsync();
    }

    //Handling Disconnection from WebSocket Connection
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User.GetUserId();
        var chatId = Context.User.GetChatId();
        var connectionId = Context.ConnectionId;
        bool isNowOffline = false;

        if (Connections.TryGetValue(userId, out List<string> connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                //Check if there are no more active connections for the user if true then notify all clients that the user is offline
                if (connections.Count == 0)
                {
                    Connections.TryRemove(userId, out _);
                    isNowOffline = true;
                }
            }

        }
        ActiveConversation.TryRemove(connectionId, out _);
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            Context.User.GetUserId(),
            Context.ConnectionAborted
        );

        if (isNowOffline)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.LastSeen = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();
            }

            var status = new { ChatId = chatId, IsOnline = false };
            await Clients.All.SendAsync("IsOnline", status);
        }

        await base.OnDisconnectedAsync(exception);
    }

    //Checking Online Status of Users
    public async Task CheckOnline(List<string> chatIds) 
    {
        if (chatIds == null || chatIds.Count == 0) return;

        var userMappings = await _context.Users
        .AsNoTracking()
        .Where(u => chatIds.Contains(u.ChatId))
        .Select(u => new { u.Id, u.ChatId, u.LastSeen })
        .ToListAsync();

        var statuses = new List<object>();

        foreach (var mapping in userMappings)
        {
            bool isOnline = IsUserOnlineInMemory(mapping.Id);

            //Prepare List of ChatId and online check to send to frontend
            DateTimeOffset? lastSeen = isOnline ? (DateTimeOffset?)null : mapping.LastSeen;
            statuses.Add(new { ChatId = mapping.ChatId, IsOnline = isOnline, LastSeen = lastSeen });
        }

        //Send the compiled list of statuses back to the requesting user
        await Clients.Caller.SendAsync("CheckOnline", statuses);
    }

    //Joining Conversation Group
    public async Task JoinConversation(string newConversationId)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User.GetUserId();
        var conversation = await _context.Conversation
            .Include(c => c.User1Navigation) 
            .Include(c => c.User2Navigation)
            .FirstOrDefaultAsync(c =>
                c.ConversationId == newConversationId &&
                (c.User1 == userId || c.User2 == userId)
            );
        if (conversation == null) throw new HubException("Unauthorized to Join Conversation");

        if (ActiveConversation.TryGetValue(connectionId, out string previousConversationId))
        {
            if (previousConversationId != newConversationId)
            {
                await Groups.RemoveFromGroupAsync(connectionId, previousConversationId);
            }
        }

        await Groups.AddToGroupAsync(connectionId, newConversationId);

        ActiveConversation.AddOrUpdate(connectionId, newConversationId, (key, oldValue) => newConversationId);
        await MarkAsRead(newConversationId);
    }

    //Leaving Conversation Group
    public async Task LeaveConversation(string conversationId)
    {
        var connectionId = Context.ConnectionId;
        await Groups.RemoveFromGroupAsync(connectionId, conversationId);
        ActiveConversation.TryRemove(connectionId, out _);
    }
    
    //Sending Message To the User
    public async Task SendMessageToUser(string conversationId, string receiverChatId, string messageContent)
    {
        if (string.IsNullOrWhiteSpace(messageContent))
            throw new HubException("Message cannot be empty.");

        var senderUserId = Context.User.GetUserId();
        var senderChatId = Context.User.GetChatId();

        var receiver = await _context.User
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ChatId == receiverChatId);
        var sender = await _context.User
            .FindAsync(senderUserId);
        if (sender == null) throw new HubException("Sender not found.");
        if (receiver == null) throw new HubException("Recipient not found.");

        var conversation = await _context.Conversation.
            FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        if (conversation == null) throw new HubException("Conversation not found.");

        var receiverUserId = receiver.Id;
        
        bool receiverIsOnline = IsUserOnlineInMemory(receiverUserId);
        bool receiverIsActiveInConversation = IsUserActiveInConversation(receiverUserId, conversationId);

        if (!(
            conversation.User1 == senderUserId ||
            conversation.User2 == senderUserId
           ))
            throw new HubException("Unaouthorized to send message to conversation");

        if (!(
            conversation.User1 == receiverUserId ||
            conversation.User2 == receiverUserId
           ))
            throw new HubException("User does not belong to this conversation");

        var nowTime = DateTimeOffset.UtcNow;

        var message = new Messages
        {
            ConversationId = conversation.Id,
            SenderId = senderUserId,
            ReceiverId = receiverUserId,
            MessageContent = messageContent,
            Status = MessageStatus.Sent,
            SentTime = nowTime,
        };

        await _context.Messages.AddAsync(message);

        if (receiverIsOnline)
        {
            message.Status = MessageStatus.Delivered;
            message.DeliveredTime = nowTime;
        }

        if (receiverIsActiveInConversation)
        {
            message.Status = MessageStatus.Read;
            message.ReadTime = nowTime;
        }

        conversation.LastMessageTime = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        //Dto to send back to clients
        var messageDto = new MessagesDto(message, senderChatId, receiverChatId);
        var senderConversationDto = new ConversationDto(conversation, receiver);
        var receiverConversationDto = new ConversationDto(conversation, sender);
        var snippet = messageContent.ToStringSnippet();

        //Send the message to client
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", conversationId, messageDto, senderChatId, message.Id);

        //Confirm to sender the message has been sent
        await Clients.Caller.SendAsync("SentMessage", conversation.Id, messageDto, message.Id);

        //Send Conversation Back to Client to Update User List and Bring Conversation to Top
        await Clients.Users(senderUserId).SendAsync("UpdateUserList", 
            senderConversationDto, snippet, 0);

        var receiverUnread = await _context.Messages.CountAsync(m =>
        m.ConversationId == conversation.Id &&
        m.ReceiverId == receiverUserId &&
        m.Status != MessageStatus.Read);

        await Clients.Users(receiverUserId).SendAsync("UpdateUserList", receiverConversationDto, snippet, receiverUnread);

        if (!receiverIsActiveInConversation)
        {
            var totalUnread = await UnreadMessagesCount(receiverUserId);
            await Clients.User(receiverUserId).SendAsync("UnreadMessagesCount", totalUnread);
        }
    }

    //Notifying Sender that message is read
    public async Task MarkAsRead(string conversationId)
    {
        var currentUserId = Context.User.GetUserId();
        var readTime = DateTimeOffset.UtcNow;

        var conversation = await _context.Conversation
            .Include(c => c.User1Navigation) 
            .Include(c => c.User2Navigation)
            .FirstOrDefaultAsync(
                u => u.ConversationId == conversationId &&
                (u.User1 == currentUserId || 
                 u.User2 == currentUserId)
            );
        if (conversation == null) throw new HubException("Unaouthorized Conversation");

        var messageToRead = await _context.Messages
            .Where(
                m => m.ConversationId == conversation.Id && 
                m.ReceiverId == currentUserId && 
                m.Status != MessageStatus.Read
            )
            .ToListAsync();
        if (messageToRead.Count == 0) return;


        foreach (var message in messageToRead)
        {
            message.Status = MessageStatus.Read;
            message.ReadTime = readTime;
        }

        var updatedMessages = messageToRead.Select(m => new
        {
            Id = m.Id,
            Status = MessageStatus.Read,
            readTime = m.ReadTime,
        });
            
        await _context.SaveChangesAsync();
        var senderId = messageToRead.First().SenderId;
        
        var latestMessage = messageToRead
            .OrderByDescending(m => m.SentTime).FirstOrDefault();

        var snippet = latestMessage.MessageContent.ToStringSnippet();

        var otherUser = conversation.User1 == currentUserId
            ? conversation.User2Navigation
            : conversation.User1Navigation;

        var conversationDto = new ConversationDto(conversation, otherUser);

        await Clients.Group(conversationId).SendAsync("MarkAsRead", conversationId, updatedMessages);

        await Clients.Caller.SendAsync("UpdateUserList", conversationDto, snippet, 0);

        var totalUnread = await UnreadMessagesCount(currentUserId);
        await Clients.User(currentUserId).SendAsync("UnreadMessagesCount", totalUnread);
    }

    public async Task Typing(string conversationId)
    {
        var senderChatId = Context.User.GetChatId();
        await Clients.Group(conversationId).SendAsync("IsTyping", conversationId, senderChatId);
    }

    //Helper Function Called By the SendMessageToUser Method to check if the user is currently active in the conversation
    private static bool IsUserActiveInConversation(string userId, string conversationId)
    {
        // Check all connections for the recipient
        if (Connections.TryGetValue(userId, out List<string> connections))
        {
            lock (connections)
            {
                foreach (var connectionId in connections)
                {
                    if (
                        ActiveConversation.TryGetValue
                        (connectionId, out string activeConversationId)
                        && activeConversationId == conversationId
                    )
                    {
                        return true; 
                    }
                }
            }
        }
        return false;
    }

    // Helper method to check in-memory connection status
    private static bool IsUserOnlineInMemory(string userId)
    {
        return Connections.ContainsKey(userId) && Connections[userId].Count > 0;
    }

    // Helper method to get total unread count
    public async Task<int> UnreadMessagesCount(string userId)
    {
        var count = await _context.Messages
            .AsNoTracking()
            .Where(
                m => m.ReceiverId == userId &&
                m.Status != MessageStatus.Read
            ).CountAsync();

        return count;
    }
 
    public string GetConnectionId() => Context.ConnectionId;

    public static List<string> GetConnectionIds()
    {
        return Connections.Keys.ToList();
    }
}

