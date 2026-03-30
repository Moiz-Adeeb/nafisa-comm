using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Carts.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Carts.Commands;

public class UpdateCartRequestModel : IRequest<UpdateCartResponseModel>
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartRequestModelValidator : AbstractValidator<UpdateCartRequestModel>
{
    public UpdateCartRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
        RuleFor(x => x.Quantity).Required().Min(1).Max(100);
    }
}

public class UpdateCartRequestHandler : IRequestHandler<UpdateCartRequestModel, UpdateCartResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public UpdateCartRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<UpdateCartResponseModel> Handle(
        UpdateCartRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        
        var product = await _context.Product.FirstOrDefaultAsync(p => 
                p.Id == request.ProductId && 
                p.IsActive == true && 
                p.IsDeleted == false, 
                cancellationToken
            );
        if (product == null) throw new NotFoundException(nameof(request.ProductId));

        if (product.Stock == 0) 
            throw new BadRequestException($"Insufficient Stock. {product.Name} is not Available in stock");
        
        if (product.Stock < request.Quantity) 
            throw new BadRequestException($"Insufficient Stock. There are only {product.Stock} items left for {product.Name}"); 
        
        var cart = await _context.Cart
            .Include(c => c.Product)
                .ThenInclude(p => p.Images.Where(i => i.IsMain && !i.IsDeleted))
            .FirstOrDefaultAsync(c => 
                c.UserId == userId && 
                c.ProductId == request.ProductId &&
                c.IsDeleted == false,   
                cancellationToken
            );
        if (cart == null) throw new NotFoundException("This item is not in your cart.");

        cart.Quantity = request.Quantity;

        _context.Cart.Update(cart);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCartResponseModel() { Data = new CartDto(cart) };
    }
}

public class UpdateCartResponseModel
{
    public CartDto Data { get; set; }
}
