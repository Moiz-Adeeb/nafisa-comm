using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Orders.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Orders.Commands;

public class CreateOrderRequestModel : IRequest<CreateOrderResponseModel>
{
    public string City { get; set; }
    public int PostalCode { get; set; }
    public string DeliveryAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderRequestModelValidator : AbstractValidator<CreateOrderRequestModel>
{
    public CreateOrderRequestModelValidator()
    {
        RuleFor(x => x.City).Required();
        RuleFor(x => x.DeliveryAddress).Required();
        RuleFor(x => x.PhoneNumber).Required();
        RuleFor(x => x.ProductId).Required();
        RuleFor(x => x.Quantity).Required().Min(1).Max(100);
    }
}

public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequestModel, CreateOrderResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CreateOrderRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CreateOrderResponseModel> Handle(
        CreateOrderRequestModel request,
        CancellationToken cancellationToken
    )
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        var userId = _sessionService.GetUserId();

        try
        {
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
            
            var effectiveUnitPrice = product.DiscountPrice ?? product.Price;
            var totalItemPrice = effectiveUnitPrice * request.Quantity;

            
            product.Stock -= request.Quantity;
            product.SoldQuantity += request.Quantity;
            
            var order = new Order()
            {
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks.ToString().Substring(10)}", 
                Status = OrderStatus.Pending,   
                City = request.City,
                PostalCode = request.PostalCode,
                PhoneNumber = request.PhoneNumber,
                DeliveryAddress = request.DeliveryAddress,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = request.Quantity,
                        UnitPrice = effectiveUnitPrice, 
                        TotalPrice = totalItemPrice,
                        Product = product 
                    }
                },
                TotalAmount = totalItemPrice
            };

            await _context.Order.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CreateOrderResponseModel() { Data = new OrderDto(order) };
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

public class CreateOrderResponseModel
{
    public OrderDto Data { get; set; }
}
