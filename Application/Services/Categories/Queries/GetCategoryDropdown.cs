using System.Linq.Expressions;
using Application.Interfaces;
using Application.Extensions;
using Application.Services.Categories.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Queries;

public class GetCategoryDropDownRequestModel : IRequest<GetCategoryDropDownResponseModel>
{
    public string Search { get; set; }
    public int? Limit { get; set; }
    public string ParentCategoryId { get; set; }
}

public class GetCategoryDropDownRequestModelValidator : AbstractValidator<GetCategoryDropDownRequestModel>
{
    public GetCategoryDropDownRequestModelValidator()
    {
        RuleFor(x => x.ParentCategoryId).Required();
    }
}

public class GetCategoryDropDownRequestHandler
    : IRequestHandler<GetCategoryDropDownRequestModel, GetCategoryDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCategoryDropDownRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCategoryDropDownResponseModel> Handle(
        GetCategoryDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Category, bool>> query = p => true;
        if (request.Search.IsNotNullOrWhiteSpace())
            query = query.AndAlso(p => p.Name.ToLower().Contains(request.Search));
        if (request.ParentCategoryId.IsNotNullOrWhiteSpace())
            query = query.AndAlso(p => p.ParentCategoryId == request.ParentCategoryId);

        var queryable = _context.Category.GetAllReadOnly(query);
        if (request.Limit.HasValue) queryable = queryable.Take(request.Limit.Value);

        var list = await queryable.Select(CategorySelector.SelectorDropDown)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return new GetCategoryDropDownResponseModel() { Data = list };
    }
}

public class GetCategoryDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
