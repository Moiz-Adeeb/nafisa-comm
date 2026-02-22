using Application.Services.Conversations.Queries;
using Application.Services.Message.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/messages")]
public class MessagesController : BaseController
{
    /// <summary>
    /// Get Messages of the selected Conversation
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet()]
    public async Task<GetMessageResponseModel> GetMessages([FromQuery] GetMessageRequestModel model) 
    {
        //var model = new GetMessageRequestModel() { ConversationId = id };
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Get Total Count of Unread Messages the user has
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("unread")]
    public async Task<GetUnreadMessageResponseModel> GetUnreadMessages([FromQuery] GetUnreadMessageRequestModel model)
    {
        return await Mediator.Send(model);
    }
}
