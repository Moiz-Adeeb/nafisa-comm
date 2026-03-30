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

namespace Application.Services.Carts.Commands;

public class DeleteCartRequestModel : IRequest<DeleteCartResponseModel>
{
    public string ProductId { get; set; }
}

public class DeleteCartRequestModelValidator : AbstractValidator<DeleteCartRequestModel>
{
    public DeleteCartRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
    }
}

public class DeleteCartRequestHandler
    : IRequestHandler<DeleteCartRequestModel, DeleteCartResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IServiceProvider _serviceProvider;

    public DeleteCartRequestHandler(
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

    public async Task<DeleteCartResponseModel> Handle(
        DeleteCartRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var cart = await _context.Cart
            .Include(p => p.User)
            .FirstOrDefaultAsync(p =>
                p.ProductId == request.ProductId &&
                p.UserId == _sessionService.GetUserId() &&
                p.IsDeleted == false,
                cancellationToken
            );
        
        if (cart == null) throw new NotFoundException(nameof(cart));
        cart.IsDeleted = true;
        cart.Quantity = 0;
        
        _context.Cart.Update(cart);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new DeleteCartResponseModel();
    }
}

public class DeleteCartResponseModel { }
