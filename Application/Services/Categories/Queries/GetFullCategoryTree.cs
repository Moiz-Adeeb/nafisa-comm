using Application.Services.Categories.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Queries;

public class GetFullCategoryTreeRequestModel : IRequest<GetFullCategoryTreeResponseModel>{  }

public class GetFullCategoryTreeRequestModelValidator : AbstractValidator<GetFullCategoryTreeRequestModel>
{ public GetFullCategoryTreeRequestModelValidator() {  } }

public class GetFullCategoryTreeRequestHandler : IRequestHandler<GetFullCategoryTreeRequestModel, GetFullCategoryTreeResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetFullCategoryTreeRequestHandler (
        ApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<GetFullCategoryTreeResponseModel> Handle(
        GetFullCategoryTreeRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var allCategories = await _context.Category
            .GetAllReadOnly()
            .OrderBy(c => c.Name)
            .Select(CategorySelector.SelectorDetail)
            .ToListAsync(cancellationToken);
        
        return new GetFullCategoryTreeResponseModel()
        {
            Data = CategorySelector.CategoryTreeBuilder.BuildTree(allCategories)
        };
    }
}

public class GetFullCategoryTreeResponseModel
{
    public List<CategoryDetailDto> Data { get; set; }
}