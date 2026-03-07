using Application.Exceptions;
using Application.Interfaces;
using Application.Services.Categories.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Queries;

public class GetCategoryByIdRequestModel : IRequest<GetCategoryByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetCategoryByIdRequestModelValidator : AbstractValidator<GetCategoryByIdRequestModel>
{
    public GetCategoryByIdRequestModelValidator() { }
}

public class GetCategoryByIdRequestHandler
    : IRequestHandler<GetCategoryByIdRequestModel, GetCategoryByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCategoryByIdRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCategoryByIdResponseModel> Handle(
        GetCategoryByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Category.GetByWithSelectAsync(
            p => p.Id == request.Id,
            CategorySelector.SelectorDetail,
            cancellationToken: cancellationToken
        );
        if (data == null) throw new NotFoundException(nameof(Category));
        
        return new GetCategoryByIdResponseModel() { Data = data };
    }
}

public class GetCategoryByIdResponseModel
{
    public CategoryDetailDto Data { get; set; }
}