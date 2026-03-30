using Application.Services.Reviews.Commands;
using Application.Services.Reviews.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/reviews")]
public class ReviewController : BaseController
{
    /// <summary>
    /// Get Reviews of a Product
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<GetReviewsResponseModel> GetReviews([FromQuery] GetReviewsRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    
    /// <summary>
    /// Get Review By Id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{productId}")]
    public async Task<GetReviewByIdResponseModel> GetReviewById([FromRoute] string productId)
    {
        var model =  new GetReviewByIdRequestModel{ ProductId = productId };
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Create Review of a Product
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<CreateReviewResponseModel> CreateReview([FromBody] CreateReviewRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Update Review of a Product
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("{productId}")]
    public async Task<UpdateReviewResponseModel> UpdateReview([FromRoute] string productId, [FromBody] UpdateReviewRequestModel model)
    {
        model.ProductId = productId;
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Delete Review of a Product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleNames.Administrator)]
    [HttpDelete("{productId}")]
    public async Task<DeleteReviewResponseModel> DeleteReview([FromRoute] string productId)
    {
        return await Mediator.Send(new DeleteReviewRequestModel { ProductId = productId });
    }
}
