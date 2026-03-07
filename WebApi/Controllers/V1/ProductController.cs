using Application.Services.Products.Commands;
using Application.Services.Products.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/products")]
public class ProductController : BaseController
{
    /// <summary>
    /// Get Products
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<GetProductsResponseModel> GetProducts([FromQuery] GetProductsRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Get Products of a Category
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("/category/{id}")]
    public async Task<GetProductsOfCategoryResponseModel> GetProductsOfCategory([FromRoute] string id, [FromQuery] GetProductsOfCategoryRequestModel model)
    {
        model.Id = id;
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Get Product By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<GetProductByIdResponseModel> GetCategoryById([FromRoute] string id)
    {
        var model =  new GetProductByIdRequestModel{ Id = id };
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Create Category
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPost]
    public async Task<CreateProductResponseModel> CreateCategory([FromBody] CreateProductRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    ///// <summary>
    ///// Update Category
    ///// </summary>
    ///// <param name="id"></param>
    ///// <param name="model"></param>
    ///// <returns></returns>
    //[Authorize]
    //[HttpPut("{id}")]
    //public async Task<UpdateCategoryResponseModel> UpdateCategory([FromRoute] string id, [FromBody] UpdateCategoryRequestModel model)
    //{
    //    model.Id = id;
    //    return await Mediator.Send(model);
    //}
    
    ///// <summary>
    ///// Delete Category
    ///// </summary>
    ///// <param name="id"></param>
    ///// <returns></returns>
    //[Authorize]
    //[HttpDelete("{id}")]
    //public async Task<DeleteCategoryResponseModel> DeleteCategory([FromRoute] string id)
    //{
    //    return await Mediator.Send(new DeleteCategoryRequestModel { Id = id });
    //}
    

    
}
