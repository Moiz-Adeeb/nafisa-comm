using Application.Exceptions;
using Application.Interfaces;
using Application.Services.Conversations.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Conversations.Queries;

public class GetConversationByIdRequestModel : IRequest<GetConversationByIdResponseModel>
{
    public string ConversationId { get; set; }
}

public class GetConversationByIdRequestModelValidator : AbstractValidator<GetConversationByIdRequestModel>
{
    public GetConversationByIdRequestModelValidator() {
        RuleFor(x => x.ConversationId).NotEmpty();
    }
}

public class GetConversationByIdRequestHandler
    : IRequestHandler<GetConversationByIdRequestModel, GetConversationByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetConversationByIdRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetConversationByIdResponseModel> Handle(
        GetConversationByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Conversation.GetByWithSelectAsync(
            p => p.ConversationId == request.ConversationId,
            ConversationSelector.SelectorDetail,
            cancellationToken: cancellationToken
        );
        if (data == null)
        {
            throw new NotFoundException(nameof(User));
        }
        return new GetConversationByIdResponseModel() { Data = data };
    }
}

public class GetConversationByIdResponseModel
{
    public ConversationDetailDto Data { get; set; }
}
