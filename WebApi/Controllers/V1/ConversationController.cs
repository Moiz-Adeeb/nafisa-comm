using Application.Services.Conversations.Commands;
using Application.Services.Conversations.Queries;
using Common.Extensions;
using Domain.Constant;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extension;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/Conversations")]
public class ConversationController : BaseController
{
    /// <summary>
    /// Get Conversations
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("all")]
    public async Task<GetConversationsResponseModel> GetConversations([FromQuery] GetConversationsRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Get Conversaton By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<GetConversationByIdResponseModel> GetConversationById([FromRoute] string id)
    {
        var model = new GetConversationByIdRequestModel() { ConversationId = id };
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Initiate Conversation By ChatId
    /// </summary>
    /// <param name="chatId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("initiate/{chatId}")]
    public async Task<InitializeConversationResponseModel> InitiateConversation([FromRoute]  string chatId)
    {
        var model = new InitializeConversationRequestModel() { ChatId = chatId };
        return await Mediator.Send(model);
    }
}
