using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Reviews.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Reviews.Commands;

public class CreateReviewRequestModel : IRequest<CreateReviewResponseModel>
{
    public string ProductId { get; set; }
    public string ProductReview { get; set; }
    public decimal Rating { get; set; }
}

public class CreateReviewRequestModelValidator : AbstractValidator<CreateReviewRequestModel>
{
    public CreateReviewRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
        RuleFor(x => x.ProductReview).Required().Max(500);
        RuleFor(x => x.Rating).Required().Min(0).Max(5);
    }
}

public class CreateReviewRequestHandler : IRequestHandler<CreateReviewRequestModel, CreateReviewResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CreateReviewRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CreateReviewResponseModel> Handle(
        CreateReviewRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        
        var productCheck = await _context.Review
            .ActiveAny(c => c.Id == request.ProductId, cancellationToken);
        
        var existCheck = await _context.Review.ActiveAny(p => 
                p.UserId == userId && 
                p.ProductId == request.ProductId, 
                cancellationToken
            );

        if (existCheck) throw new AlreadyExistsException(nameof(request.ProductReview));


        if (productCheck) throw new NotFoundException(nameof(request.ProductId));

        var review = new Review()
        {
            ProductReview = request.ProductReview,
            Rating = request.Rating,
            ProductId = request.ProductId,
            UserId = userId,
        };

        await _context.Review.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateReviewResponseModel() { Data = new ReviewDto(review) };
    }
}

public class CreateReviewResponseModel
{
    public ReviewDto Data { get; set; }
}
