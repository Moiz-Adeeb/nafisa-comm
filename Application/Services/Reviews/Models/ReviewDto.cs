using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Services.Products.Models;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Reviews.Models;

public class ReviewDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string? Description { get; set; }
    public string ProductId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public ReviewDto() { }

    public ReviewDto(Review review)
    {
        Id = review.Id;
        Description = review.ProductReview;
        ProductId = review.ProductId;
        CreatedDate = review.CreatedDate;
    }
}

public class ReviewDetailDto : ReviewDto
{
    public ProductDto Product { get; set; }
    
    public ReviewDetailDto() { }

    public ReviewDetailDto(Review review)
    {
        Id = review.Id;
        Description = review.ProductReview;
        ProductId = review.ProductId;
        CreatedDate = review.CreatedDate;
    }
}

public class ReviewSelector
{
    public static readonly Expression<Func<Review, ReviewDto>> Selector = r => new ReviewDto()
    {
        Id = r.Id,
        UserName = r.User.Name,
        Description = r.ProductReview,
        ProductId = r.ProductId,
        CreatedDate = r.CreatedDate,
    };
    public static readonly Expression<Func<Review, ReviewDetailDto>> SelectorDetail = r => new ReviewDetailDto()
    {
        Id = r.Id,
        UserName = r.User.Name,
        Description = r.ProductReview,
        ProductId = r.ProductId,
        Product =
        {
            Name = r.Product.Name,
            Description = r.Product.Description,
            Image = r.Product.Images
                .Where(i => i.IsDeleted == false)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    Url = i.Url,
                    IsMain = i.IsMain
                }).FirstOrDefault(p => p.IsMain),
            Price = r.Product.Price,
            DiscountPrice = r.Product.DiscountPrice,
            CategoryId = r.Product.CategoryId,
            
        },
        CreatedDate = r.CreatedDate,
    };

    public static readonly Expression<Func<Review, DropDownDto<string>>> SelectorDropDown = p => new DropDownDto<string>() { Name = p.User.Name, Id = p.Id };
}

