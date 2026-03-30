using System.Linq.Expressions;
using Application.Services.Products.Models;
using Application.Services.WishLists.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.WishLists.Queries;

public class GetProductsOfWishListRequestModel : GetPagedRequest<GetProductsOfWishListResponseModel> { }

public class GetProductsOfWishListRequestModelValidator : PageRequestValidator<GetProductsOfWishListRequestModel>
{
    public GetProductsOfWishListRequestModelValidator() {  }
}

public class GetProductsOfWishListRequestHandler : IRequestHandler<GetProductsOfWishListRequestModel, GetProductsOfWishListResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetProductsOfWishListRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductsOfWishListResponseModel> Handle(
        GetProductsOfWishListRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<WishList, bool>> query = p => true;
        
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(w =>
                w.Product.Name.Contains(request.Search) || w.Product.Name.ToLower().Contains(request.Search)
            );
        }
        
        var list = await _context.WishList.GetManyReadOnly(query, request)
            .Select(WishListSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.WishList.ActiveCount(query, cancellationToken: cancellationToken);
        
        return new GetProductsOfWishListResponseModel() { Data = list, Count = count };
    }
}

public class GetProductsOfWishListResponseModel
{
    public List<WishListDto> Data { get; set; }
    public int Count { get; set; }
}