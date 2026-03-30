using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Reviews.Models;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Reviews.Commands;

public class UpdateReviewRequestModel : IRequest<UpdateReviewResponseModel>
{
    public string ProductReview { get; set; }
    public decimal Rating { get; set; }
    public string ProductId { get; set; }
}

public class UpdateReviewRequestModelValidator : AbstractValidator<UpdateReviewRequestModel>
{
    public UpdateReviewRequestModelValidator()
    {
        RuleFor(x => x.ProductReview).Required().Max(500);
        RuleFor(x => x.Rating).Required().Min(0).Max(5);
        RuleFor(x => x.ProductId).Required();
    }
}

public class UpdateReviewRequestHandler
    : IRequestHandler<UpdateReviewRequestModel, UpdateReviewResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public UpdateReviewRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IImageService imageService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _imageService = imageService;
    }

    public async Task<UpdateReviewResponseModel> Handle(
        UpdateReviewRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var review = await _context.Review
            .Include(r => r.User) 
            .FirstOrDefaultAsync(r => 
                r.ProductId == request.ProductId && 
                r.UserId == _sessionService.GetUserId(),
                cancellationToken
            );

        if (review == null) throw new NotFoundException(nameof(request.ProductId));

        review.ProductReview = request.ProductReview;
        review.Rating = request.Rating;
            
        _context.Review.Update(review);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdateReviewResponseModel() { Data = new ReviewDto(review) };
    }
}

public class UpdateReviewResponseModel
{
    public ReviewDto Data { get; set; }
}