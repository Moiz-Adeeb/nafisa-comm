using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Products.Models;
using Application.Shared;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Orders.Models;

public class OrderDto
{
    public string Id { get; set; }
    public string OrderNumber { get; set; }
    public string City { get; set; }
    public int? PostalCode { get; set; }
    public string DeliveryAddress { get; set; }
    public string PhoneNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    
    public OrderDto() { }

    public OrderDto(Order order)
    {
        Id = order.Id;
        OrderNumber = order.OrderNumber;
        City = order.City;
        PostalCode = order.PostalCode;
        DeliveryAddress = order.DeliveryAddress;
        PhoneNumber = order.PhoneNumber;
        TotalAmount = order.TotalAmount;
        Status = order.Status;
        OrderDate = order.OrderDate;
        OrderItems = order.OrderItems
            .Select(i => new OrderItemDto(i))
            .ToList();
    }
}

public class OrderDetailDto : OrderDto
{
    public OrderDetailDto() { }

    public OrderDetailDto(Order order)
    {
        Id = order.Id;
        OrderNumber = order.OrderNumber;
        City = order.City;
        PostalCode = order.PostalCode;
        DeliveryAddress = order.DeliveryAddress;
        PhoneNumber = order.PhoneNumber;
        TotalAmount = order.TotalAmount;
        Status = order.Status;
        OrderDate = order.OrderDate;
        OrderItems = order.OrderItems
            .Select(i => new OrderItemDto(i))
            .ToList();
    }
}

public class OrderSelector
{
    public static readonly Expression<Func<Order, OrderDto>> Selector = o => new OrderDto()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        City = o.City,
        PostalCode = o.PostalCode,
        DeliveryAddress = o.DeliveryAddress,
        PhoneNumber = o.PhoneNumber,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        OrderDate = o.OrderDate,
        OrderItems = o.OrderItems
            .Select(i => new OrderItemDto()
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Product = new ProductDto()
                {
                    Id = i.Product.Id,
                    Name = i.Product.Name,
                    Image = i.Product.Images
                        .Where(i => !i.IsDeleted && i.IsMain)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            Url = i.Url,
                            IsMain = i.IsMain
                        })
                        .FirstOrDefault(i => i.IsMain),
                }
            })
            .ToList(),
    };
    public static readonly Expression<Func<Order, OrderDetailDto>> SelectorDetail = o => new OrderDetailDto()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        City = o.City,
        PostalCode = o.PostalCode,
        DeliveryAddress = o.DeliveryAddress,
        PhoneNumber = o.PhoneNumber,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        OrderDate = o.OrderDate,
        OrderItems = o.OrderItems
            .Select(i => new OrderItemDto()
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Product = new ProductDto()
                {
                    Id = i.Product.Id,
                    Name = i.Product.Name,
                    Image = i.Product.Images
                        .Where(i => !i.IsDeleted && i.IsMain)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            Url = i.Url,
                            IsMain = i.IsMain
                        })
                        .FirstOrDefault(i => i.IsMain),
                }
            })
            .ToList(),
    };

    public static readonly Expression<Func<Order, DropDownDto<string>>> SelectorDropDown = p => new DropDownDto<string>() { Name = p.User.Name, Id = p.Id };
}

