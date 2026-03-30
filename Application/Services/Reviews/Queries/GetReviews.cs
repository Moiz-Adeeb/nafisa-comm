using System.Linq.Expressions;
using Application.Extensions;
using Application.Services.Reviews.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Reviews.Queries;

public class GetReviewsRequestModel : GetPagedRequest<GetReviewsResponseModel>
{
    public string ProductId { get; set; }
}

public class GetReviewsRequestModelValidator : PageRequestValidator<GetReviewsRequestModel>
{
    public GetReviewsRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
    }
}

public class GetReviewsRequestHandler : IRequestHandler<GetReviewsRequestModel, GetReviewsResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetReviewsRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetReviewsResponseModel> Handle(
        GetReviewsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Review, bool>> query = p => true;   
        query = query.AndAlso(p => p.ProductId == request.ProductId);
        
        var list = await _context.Review.GetManyReadOnly(query, request)
            .Select(ReviewSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Review.ActiveCount(query, cancellationToken: cancellationToken);
        
        return new GetReviewsResponseModel() { Data = list, Count = count };
    }
}

public class GetReviewsResponseModel
{
    public List<ReviewDto> Data { get; set; }
    public int Count { get; set; }
}