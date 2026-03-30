using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Reviews.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Reviews.Queries;

public class GetReviewByIdRequestModel : IRequest<GetReviewByIdResponseModel>
{
    public string ProductId { get; set; }
}

public class GetReviewByIdRequestModelValidator : AbstractValidator<GetReviewByIdRequestModel>
{
    public GetReviewByIdRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
    }
}

public class GetReviewByIdRequestHandler
    : IRequestHandler<GetReviewByIdRequestModel, GetReviewByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetReviewByIdRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetReviewByIdResponseModel> Handle(
        GetReviewByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Review
            .GetByWithSelectAsync( r => 
            r.ProductId == request.ProductId &&
            r.UserId == _sessionService.GetUserId(),
            ReviewSelector.SelectorDetail,
            cancellationToken: cancellationToken
        );
        if (data == null) throw new NotFoundException(nameof(Review));
        
        return new GetReviewByIdResponseModel() { Data = data };
    }
}

public class GetReviewByIdResponseModel
{
    public ReviewDetailDto Data { get; set; }
}