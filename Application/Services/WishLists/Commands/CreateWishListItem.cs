using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.WishLists.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.WishLists.Commands;

public class CreateWishListRequestModel : IRequest<CreateWishListResponseModel>
{
    public string ProductId { get; set; }
}

public class CreateWishListRequestModelValidator : AbstractValidator<CreateWishListRequestModel>
{
    public CreateWishListRequestModelValidator()
    {
        RuleFor(x => x.ProductId).Required();
    }
}

public class CreateWishListRequestHandler : IRequestHandler<CreateWishListRequestModel, CreateWishListResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CreateWishListRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CreateWishListResponseModel> Handle(
        CreateWishListRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        
        var productCheck = await _context.WishList
            .ActiveAny(p => p.Id == request.ProductId, cancellationToken);
        if (productCheck) throw new NotFoundException(nameof(request.ProductId));
        
        var existCheck = await _context.WishList.ActiveAny(w => 
                w.UserId == userId && 
                w.ProductId == request.ProductId,   
                cancellationToken
            );
        if (existCheck) throw new AlreadyExistsException(nameof(request.ProductId));

        var wishList = new WishList()
        {
            ProductId = request.ProductId,
            UserId = userId,
        };

        await _context.WishList.AddAsync(wishList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateWishListResponseModel() { Data = new WishListDto(wishList) };
    }
}

public class CreateWishListResponseModel
{
    public WishListDto Data { get; set; }
}
