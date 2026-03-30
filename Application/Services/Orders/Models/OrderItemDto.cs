using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Products.Models;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Orders.Models;

public class OrderItemDto 
{
    public string Id { get; set; }
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public ProductDto Product { get; set; }
    
    public OrderItemDto() { }

    public OrderItemDto(OrderItem orderItem)
    {
        Id = orderItem.Id;
        OrderId = orderItem.OrderId;
        ProductId = orderItem.ProductId;
        Quantity = orderItem.Quantity;
        UnitPrice = orderItem.UnitPrice;
        TotalPrice = orderItem.TotalPrice;
        Product = new ProductDto
        {
            Id = orderItem.Product.Id,
            Name = orderItem.Product.Name,
            Image = orderItem.Product.Images
                .Where(i => !i.IsDeleted && i.IsMain)
                .Select(i => new ProductImageDto(i))
                .FirstOrDefault(i => i.IsMain),
        };
    }
}

public class OrderItemSelector
{
    public static readonly Expression<Func<OrderItem, OrderItemDto>> Selector = o => new OrderItemDto()
    {
        Id = o.Id,
        OrderId = o.OrderId,
        ProductId = o.ProductId,
        Quantity = o.Quantity,
        UnitPrice = o.UnitPrice,
        TotalPrice = o.TotalPrice,
        Product = new ProductDto
        {   
            Id = o.Product.Id,
            Name = o.Product.Name,
            Image = o.Product.Images
                .Where(i => !i.IsDeleted && i.IsMain)
                .Select(i => new ProductImageDto(i))
                .FirstOrDefault(i => i.IsMain),
            
        }
    };
}