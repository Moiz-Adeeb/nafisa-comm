using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Products.Models;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Carts.Models;

public class CartDto : ProductDto
{
    public CartDto() { }

    public CartDto(Cart cart)
    {
        Id = cart.Product.Id;
        Name = cart.Product.Name;
        Image = cart.Product.Images
            .Where(i => !i.IsDeleted && i.IsMain)
            .Select(p => new ProductImageDto(p))
            .FirstOrDefault(p => p.IsMain);
        Price = cart.Product.Price;
        DiscountPrice = cart.Product.DiscountPrice;
        Stock = cart.Product.Stock;
        SoldQuantity = cart.Product.SoldQuantity;
        IsActive = cart.Product.IsActive;
        CategoryId = cart.Product.CategoryId;
        Rating = cart.Product.Rating;
        ReviewCount = cart.Product.Reviews.Count();
        CreatedDate = cart.Product.CreatedDate;
    }
}

public class CartDetailDto : ProductDetailDto
{
    public CartDetailDto() { }

    public CartDetailDto(Cart cart)
    {
        Id = cart.Product.Id;
        Name = cart.Product.Name;
        Description = cart.Product.Description;
        Images = cart.Product.Images
            .Where(i => !i.IsDeleted)
            .Select(p => new ProductImageDto(p))
            .ToList();
        Price = cart.Product.Price;
        DiscountPrice = cart.Product.DiscountPrice;
        Stock = cart.Product.Stock;
        SoldQuantity = cart.Product.SoldQuantity;
        IsActive = cart.Product.IsActive;
        CategoryId = cart.Product.CategoryId;
        CreatedDate = cart.Product.CreatedDate;
    }
}

public class CartSelector
{
    public static readonly Expression<Func<Cart, CartDto>> Selector = w => new CartDto()
    {
        Id = w.Product.Id,
        Name = w.Product.Name,
        Image = w.Product.Images
            .Where(i => i.IsDeleted == false)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                IsMain = i.IsMain
            }).FirstOrDefault(p => p.IsMain),
        Price = w.Product.Price,
        DiscountPrice = w.Product.DiscountPrice,
        Stock = w.Product.Stock,
        SoldQuantity = w.Product.SoldQuantity,
        IsActive = w.Product.IsActive,
        CategoryId = w.Product.CategoryId,
        Rating = w.Product.Rating,
        ReviewCount = w.Product.Reviews.Count(),
        CreatedDate = w.Product.CreatedDate,
    };
    public static readonly Expression<Func<Cart, CartDetailDto>> SelectorDetail = w => new CartDetailDto()
    {
        Id = w.Product.Id,
        Name = w.Product.Name,
        Description = w.Product.Description,
        Images = w.Product.Images
            .Where(i => i.IsDeleted == false && i.IsMain)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                IsMain = i.IsMain
            }).ToList(),
        Price = w.Product.Price,
        DiscountPrice = w.Product.DiscountPrice,
        Stock = w.Product.Stock,
        SoldQuantity = w.Product.SoldQuantity,
        IsActive = w.Product.IsActive,
        CategoryId = w.Product.CategoryId,
        Rating = w.Product.Rating,
        ReviewCount = w.Product.Reviews.Count(),
        CreatedDate = w.Product.CreatedDate,
    };

    public static readonly Expression<Func<Cart, DropDownDto<string>>> SelectorDropDown = p => new DropDownDto<string>() { Name = p.User.Name, Id = p.Id };
}

