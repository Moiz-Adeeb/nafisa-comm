using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Products.Models;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.WishLists.Models;

public class WishListDto : ProductDto
{
    public WishListDto() { }

    public WishListDto(WishList wishList)
    {
        Id = wishList.Product.Id;
        Name = wishList.Product.Name;
        Image = wishList.Product.Images
            .Where(i => !i.IsDeleted && i.IsMain)
            .Select(p => new ProductImageDto(p))
            .FirstOrDefault(p => p.IsMain);
        Price = wishList.Product.Price;
        DiscountPrice = wishList.Product.DiscountPrice;
        Stock = wishList.Product.Stock;
        SoldQuantity = wishList.Product.SoldQuantity;
        IsActive = wishList.Product.IsActive;
        CategoryId = wishList.Product.CategoryId;
        CategoryName = wishList.Product.Category.Name;
        CompanyId = wishList.Product.CompanyId;
        CompanyName = wishList.Product.Company.Name;
        Rating = wishList.Product.Rating;
        ReviewCount = wishList.Product.Reviews.Count;
        CreatedDate = wishList.Product.CreatedDate;
    }
}

public class WishListDetailDto : ProductDetailDto
{
    public WishListDetailDto() { }

    public WishListDetailDto(WishList wishList)
    {
        Id = wishList.Product.Id;
        Name = wishList.Product.Name;
        Description = wishList.Product.Description;
        Images = wishList.Product.Images
            .Where(i => !i.IsDeleted)
            .Select(p => new ProductImageDto(p))
            .ToList();
        Price = wishList.Product.Price;
        DiscountPrice = wishList.Product.DiscountPrice;
        Stock = wishList.Product.Stock;
        SoldQuantity = wishList.Product.SoldQuantity;
        IsActive = wishList.Product.IsActive;
        CategoryId = wishList.Product.CategoryId;
        CategoryName = wishList.Product.Category.Name;
        CompanyId = wishList.Product.CompanyId;
        CompanyName = wishList.Product.Company.Name;
        Rating = wishList.Product.Rating;
        ReviewCount = wishList.Product.Reviews.Count;
        CreatedDate = wishList.Product.CreatedDate;
    }
}

public class WishListSelector
{
    public static readonly Expression<Func<WishList, WishListDto>> Selector = w => new WishListDto()
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
        CategoryName = w.Product.Category.Name,
        CompanyId = w.Product.CompanyId,
        CompanyName = w.Product.Company.Name,
        Rating = w.Product.Rating,
        ReviewCount = w.Product.Reviews.Count(),
        CreatedDate = w.Product.CreatedDate,
    };
    public static readonly Expression<Func<WishList, WishListDetailDto>> SelectorDetail = w => new WishListDetailDto()
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
        CategoryName = w.Product.Category.Name,
        CompanyId = w.Product.CompanyId,
        CompanyName = w.Product.Company.Name,
        Rating = w.Product.Rating,
        ReviewCount = w.Product.Reviews.Count(),
        CreatedDate = w.Product.CreatedDate,
    };

    public static readonly Expression<Func<WishList, DropDownDto<string>>> SelectorDropDown = p => new DropDownDto<string>() { Name = p.User.Name, Id = p.Id };
}

