using System.Linq.Expressions;
using Application.Services.Products.Models;
using Application.Services.Categories.Models;
using Application.Services.Categories.Extensions;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Products.Queries;

public class GetProductsOfCategoryRequestModel : GetPagedRequest<GetProductsOfCategoryResponseModel>
{
    public string Id { get; set; }
}

public class GetProductsOfCategoryRequestModelValidator : PageRequestValidator<GetProductsOfCategoryRequestModel>
{
    public GetProductsOfCategoryRequestModelValidator() {  }
}

public class GetProductsOfCategoryRequestHandler : IRequestHandler<GetProductsOfCategoryRequestModel, GetProductsOfCategoryResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetProductsOfCategoryRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductsOfCategoryResponseModel> Handle(
        GetProductsOfCategoryRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Category, bool>> categoryfFilter = p => true;
        var allCategories = await _context.Category
            .GetAllReadOnly()
            .OrderBy(c => c.Name)
            .Select(CategorySelector.SelectorDetail)
            .ToListAsync(cancellationToken);
        
        var tree = CategorySelector.CategoryTreeBuilder.BuildTree(allCategories);
        var targetCategory = allCategories.FirstOrDefault(c => c.Id == request.Id);
        if (targetCategory == null) return new GetProductsOfCategoryResponseModel();
        
        var categoryIds = new List<string>();
        CategoryExtensions.GetAllChildIds(targetCategory, categoryIds);
        
        Expression<Func<Product, bool>> query = p => categoryIds.Contains(p.CategoryId);
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.Name.Contains(request.Search) || p.Name.ToLower().Contains(request.Search)
            );
        }
        
        var list = await _context
            .Product.GetManyReadOnly(query, request)
            .Select(ProductSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Product.ActiveCount(query, cancellationToken: cancellationToken);
        
        return new GetProductsOfCategoryResponseModel() { Data = list, Count = count };
    }
}

public class GetProductsOfCategoryResponseModel
{
    public List<ProductDto> Data { get; set; }
    public int Count { get; set; }
}