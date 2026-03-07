using Application.Exceptions;
using Application.Interfaces;
using Application.Services.Products.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Products.Queries;

public class GetProductByIdRequestModel : IRequest<GetProductByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetProductByIdRequestModelValidator : AbstractValidator<GetProductByIdRequestModel>
{
    public GetProductByIdRequestModelValidator() { }
}

public class GetProductByIdRequestHandler
    : IRequestHandler<GetProductByIdRequestModel, GetProductByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetProductByIdRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetProductByIdResponseModel> Handle(
        GetProductByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var data = await _context.Product.GetByWithSelectAsync(
            p => p.Id == request.Id,
            ProductSelector.SelectorDetail,
            cancellationToken: cancellationToken
        );
        if (data == null) throw new NotFoundException(nameof(Product));
        
        return new GetProductByIdResponseModel() { Data = data };
    }
}

public class GetProductByIdResponseModel
{
    public ProductDetailDto Data { get; set; }
}