using System.Linq.Expressions;
using Application.Services.Products.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Products.Queries;

public class GetProductsRequestModel : GetPagedRequest<GetProductsResponseModel>
{
    public string CategoryId { get; set; }
    public string CompanyId { get; set; }
}

public class GetProductsRequestModelValidator : PageRequestValidator<GetProductsRequestModel>
{
    public GetProductsRequestModelValidator() {  }
}

public class GetProductsRequestHandler : IRequestHandler<GetProductsRequestModel, GetProductsResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetProductsRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductsResponseModel> Handle(
        GetProductsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Product, bool>> query = p => true;
        if (request.CategoryId.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p => p.CategoryId == request.CategoryId);
        }        
        
        if (request.CompanyId.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p => p.CompanyId == request.CompanyId);
        }
        
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.Name.Contains(request.Search) || p.Name.ToLower().Contains(request.Search)
            );
        }
        
        var list = await _context.Product.GetManyReadOnly(query, request)
            .Select(ProductSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Product.ActiveCount(query, cancellationToken: cancellationToken);
        
        return new GetProductsResponseModel() { Data = list, Count = count };
    }
}

public class GetProductsResponseModel
{
    public List<ProductDto> Data { get; set; }
    public int Count { get; set; }
}