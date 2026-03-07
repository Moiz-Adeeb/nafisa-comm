using System.Linq.Expressions;
using Application.Services.Categories.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Queries;

public class GetCategoriesRequestModel : GetPagedRequest<GetCategoriesResponseModel>{  }

public class GetCategoriesRequestModelValidator : PageRequestValidator<GetCategoriesRequestModel>
{
    public GetCategoriesRequestModelValidator() {  }
}

public class GetCategoriesRequestHandler : IRequestHandler<GetCategoriesRequestModel, GetCategoriesResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetCategoriesRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCategoriesResponseModel> Handle(
        GetCategoriesRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Category, bool>> query = p => true;
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.Name.Contains(request.Search) || p.Name.ToLower().Contains(request.Search)
            );
        }
        
        var list = await _context
            .Category.GetManyReadOnly(query, request)
            .Select(CategorySelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Category.ActiveCount(query, cancellationToken: cancellationToken);
        
        return new GetCategoriesResponseModel() { Data = list, Count = count };
    }
}

public class GetCategoriesResponseModel
{
    public List<CategoryDto> Data { get; set; }
    public int Count { get; set; }
}