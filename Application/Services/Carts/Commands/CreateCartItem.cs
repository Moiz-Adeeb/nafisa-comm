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

public class CreateCartRequestModel : IRequest<CreateCartResponseModel>
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateCartRequestModelValidator : AbstractValidator<CreateCartRequestModel>
{
    public CreateCartRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
        RuleFor(x => x.Quantity).Required().Min(1).Max(100);
    }
}

public class CreateCartRequestHandler : IRequestHandler<CreateCartRequestModel, CreateCartResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CreateCartRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CreateCartResponseModel> Handle(
        CreateCartRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        
        var product = await _context.Product
            .Include(p => 
                p.Images.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(p => 
                p.Id == request.ProductId && 
                p.IsActive == true && 
                p.IsDeleted == false, 
                cancellationToken
            );
        if (product == null) throw new NotFoundException(nameof(request.ProductId));

        if (product.Stock == 0) throw new BadRequestException($"Insufficient Stock. {product.Name} is not Available in stock");
        
        if (product.Stock < request.Quantity) 
            throw new BadRequestException($"Insufficient Stock. There are only {product.Stock} items left for {product.Name}"); 
        
        var existCheck = await _context.Cart.ActiveAny(c => 
                c.UserId == userId && 
                c.ProductId == request.ProductId &&
                c.IsDeleted == false,   
                cancellationToken
            );
        if (existCheck) throw new AlreadyExistsException(nameof(request.ProductId));

        var cart = new Cart()
        {
            ProductId = request.ProductId,
            UserId = userId,
            Quantity = request.Quantity,
            Product = product
        };

        await _context.Cart.AddAsync(cart, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCartResponseModel() { Data = new CartDto(cart) };
    }
}

public class CreateCartResponseModel
{
    public CartDto Data { get; set; }
}
