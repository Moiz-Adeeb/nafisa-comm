using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Reviews.Commands;

public class DeleteReviewRequestModel : IRequest<DeleteReviewResponseModel>
{
    public string ProductId { get; set; }
}

public class DeleteReviewRequestModelValidator : AbstractValidator<DeleteReviewRequestModel>
{
    public DeleteReviewRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
    }
}

public class DeleteReviewRequestHandler
    : IRequestHandler<DeleteReviewRequestModel, DeleteReviewResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IServiceProvider _serviceProvider;

    public DeleteReviewRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IServiceProvider serviceProvider
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _serviceProvider = serviceProvider;
    }

    public async Task<DeleteReviewResponseModel> Handle(
        DeleteReviewRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var review = await _context.Review
            .Include(p => p.User)
            .FirstOrDefaultAsync(r =>
                r.ProductId == request.ProductId &&
                r.UserId == _sessionService.GetUserId(),
                cancellationToken
            );
        
        if (review == null) throw new NotFoundException(nameof(review));
        review.IsDeleted = true;
        
        _context.Review.Update(review);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new DeleteReviewResponseModel();
    }
}

public class DeleteReviewResponseModel { }
