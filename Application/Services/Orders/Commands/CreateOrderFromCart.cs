using System.Linq.Expressions;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Carts.Models;
using Application.Services.Orders.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Orders.Commands;

public class CreateOrderFromCartRequestModel : IRequest<CreateOrderFromCartResponseModel>
{
    public string City { get; set; }
    public int PostalCode { get; set; }
    public string DeliveryAddress { get; set; }
    public string PhoneNumber { get; set; }
}

public class CreateOrderFromCartRequestModelValidator : AbstractValidator<CreateOrderFromCartRequestModel>
{
    public CreateOrderFromCartRequestModelValidator()
    {
        RuleFor(x => x.City).Required();
        RuleFor(x => x.DeliveryAddress).Required();
        RuleFor(x => x.PhoneNumber).Required();
    }
}

public class CreateOrderFromCartRequestHandler : IRequestHandler<CreateOrderFromCartRequestModel, CreateOrderFromCartResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CreateOrderFromCartRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CreateOrderFromCartResponseModel> Handle(
        CreateOrderFromCartRequestModel request,
        CancellationToken cancellationToken
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        var userId = _sessionService.GetUserId();

        try
        {   
            var cartItems = await _context.Cart
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images
                        .Where(i => !i.IsDeleted))
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .ToListAsync(cancellationToken);
            
            if (cartItems.Count == 0) 
                throw new BadRequestException("Your cart is empty.");
            
            var orderItems = new List<OrderItem>();
            decimal orderTotalAmount = 0;
            
            foreach (var item in cartItems)
            {
                var product = item.Product;

                if (product == null || !product.IsActive || product.IsDeleted) 
                    throw new BadRequestException($"{product?.Name ?? "A product"} is no longer available.");
                if (product.Stock < item.Quantity) 
                    throw new BadRequestException($"Insufficient stock for {product.Name}. Only {product.Stock} left.");

                var effectivePrice = product.DiscountPrice ?? product.Price;
                var itemTotal = effectivePrice * item.Quantity;

                product.Stock -= item.Quantity;
                product.SoldQuantity += item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = effectivePrice,
                    TotalPrice = itemTotal,
                    Product = product 
                });

                orderTotalAmount += itemTotal;
                item.IsDeleted = true;
            }
            
            var order = new Order()
            {
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks.ToString().Substring(10)}", 
                Status = OrderStatus.Pending,   
                City = request.City,
                PostalCode = request.PostalCode,
                PhoneNumber = request.PhoneNumber,
                DeliveryAddress = request.DeliveryAddress,
                OrderDate = DateTimeOffset.UtcNow,
                OrderItems = orderItems,
                TotalAmount = orderTotalAmount
            };

            await _context.Order.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CreateOrderFromCartResponseModel() { Data = new OrderDto(order) };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new BadRequestException("The stock changed during your checkout. Please try again.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

public class CreateOrderFromCartResponseModel
{
    public OrderDto Data { get; set; }
}
