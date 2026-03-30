using Application.Services.WishLists.Commands;
using Application.Services.WishLists.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/wishLists")]
public class WishListController : BaseController
{
    /// <summary>
    /// Get WishList Products
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<GetProductsOfWishListResponseModel> GetWishListProducts([FromQuery] GetProductsOfWishListRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Create WishList Item
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("{productId}")]
    public async Task<CreateWishListResponseModel> CreateWishListItem([FromRoute] string productId )
    {
        return await Mediator.Send(new CreateWishListRequestModel { ProductId = productId });
    }
    
    /// <summary>
    /// Delete WishList Item
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("{productId}")]
    public async Task<DeleteWishListResponseModel> DeleteWishListItem([FromRoute] string productId)
    {
        return await Mediator.Send(new DeleteWishListRequestModel { ProductId = productId });
    }
}
