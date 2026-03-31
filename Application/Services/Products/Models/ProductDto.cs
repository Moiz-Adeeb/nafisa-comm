using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Reviews.Models;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Products.Models;

public class ProductDetailDto : ProductDto
{
    public List<ProductImageDto> Images { get; set; }
    public List<ReviewDto> Reviews { get; set; }
    
    public ProductDetailDto() { }

    public ProductDetailDto(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        Images = product.Images
            .Where(i => !i.IsDeleted)
            .Select(p => new ProductImageDto(p))
            .ToList();
        Price = product.Price;
        DiscountPrice = product.DiscountPrice;
        Stock = product.Stock;
        SoldQuantity = product.SoldQuantity;
        IsActive = product.IsActive;
        CategoryId = product.CategoryId;
        CategoryName = product.Category.Name;
        CompanyId = product.CompanyId;
        CompanyName = product.Company.Name;
        Rating = product.Rating;
        ReviewCount = product.Reviews.Count();
        CreatedDate = product.CreatedDate;
    }
}

public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public int SoldQuantity { get; set; }
    public bool IsActive { get; set; }
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }
    public ProductImageDto Image { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public ProductDto() { }

    public ProductDto(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Image = product.Images
            .Where(i => !i.IsDeleted && i.IsMain)
            .Select(p => new ProductImageDto(p))
            .FirstOrDefault(p => p.IsMain);
        Price = product.Price;
        DiscountPrice = product.DiscountPrice;
        Stock = product.Stock;
        SoldQuantity = product.SoldQuantity;
        IsActive = product.IsActive;
        CategoryId = product.CategoryId;
        CategoryName = product.Category.Name;
        CompanyId = product.CompanyId;
        CompanyName = product.Company.Name;
        Rating = product.Rating;
        ReviewCount = product.Reviews.Count();
        CreatedDate = product.CreatedDate;
    }
}


public class ProductSelector
{
    public static readonly Expression<Func<Product, ProductDto>> Selector = p => new ProductDto()
    {
        Id = p.Id,
        Name = p.Name,
        Image = p.Images
            .Where(i => i.IsDeleted == false)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                IsMain = i.IsMain
            }).FirstOrDefault(p => p.IsMain),
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        Stock = p.Stock,
        SoldQuantity = p.SoldQuantity,
        IsActive = p.IsActive,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        CompanyId = p.CompanyId,
        CompanyName = p.Company.Name,
        Rating = p.Rating,
        ReviewCount = p.Reviews.Count(),
        CreatedDate = p.CreatedDate,
    };
    public static readonly Expression<Func<Product, ProductDetailDto>> SelectorDetail = p => new ProductDetailDto()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Images = p.Images
            .Where(i => i.IsDeleted == false)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                IsMain = i.IsMain
            }).ToList(),
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        Stock = p.Stock,
        SoldQuantity = p.SoldQuantity,
        IsActive = p.IsActive,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        CompanyId = p.CompanyId,
        CompanyName = p.Company.Name,
        Rating = p.Rating,
        ReviewCount = p.Reviews.Count(),
        CreatedDate = p.CreatedDate,
    };

    public static readonly Expression<Func<Product, DropDownDto<string>>> SelectorDropDown = p => new DropDownDto<string>() { Name = p.Name, Id = p.Id };
}