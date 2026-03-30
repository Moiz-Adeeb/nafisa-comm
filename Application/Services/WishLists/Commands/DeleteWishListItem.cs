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

namespace Application.Services.WishLists.Commands;

public class DeleteWishListRequestModel : IRequest<DeleteWishListResponseModel>
{
    public string ProductId { get; set; }
}

public class DeleteWishListRequestModelValidator : AbstractValidator<DeleteWishListRequestModel>
{
    public DeleteWishListRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
    }
}

public class DeleteWishListRequestHandler
    : IRequestHandler<DeleteWishListRequestModel, DeleteWishListResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IServiceProvider _serviceProvider;

    public DeleteWishListRequestHandler(
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

    public async Task<DeleteWishListResponseModel> Handle(
        DeleteWishListRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var wishList = await _context.WishList
            .Include(p => p.User)
            .FirstOrDefaultAsync(r =>
                r.ProductId == request.ProductId &&
                r.UserId == _sessionService.GetUserId(),
                cancellationToken
            );
        
        if (wishList == null) throw new NotFoundException(nameof(wishList));
        wishList.IsDeleted = true;
        
        _context.WishList.Update(wishList);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new DeleteWishListResponseModel();
    }
}

public class DeleteWishListResponseModel { }
