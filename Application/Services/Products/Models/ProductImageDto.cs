using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Products.Models;

public class ProductImageDto 
{
    public string Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }
    
    public ProductImageDto() { }

    public ProductImageDto(ProductImage productImage)
    {
        Id = productImage.Id;
        Url = productImage.Url;
        IsMain = productImage.IsMain;
    }
}

public class ProductImageSelector
{
    public static readonly Expression<Func<ProductImage, ProductImageDto>> Selector = p => new ProductImageDto()
    {
        Id = p.Id,
        Url = p.Url,
        IsMain = p.IsMain
    };
}