using Application.Services.Categories.Commands;
using Application.Services.Categories.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/categories")]
public class CategoryController : BaseController
{
    /// <summary>
    /// Get Category Tree
    /// </summary>
    /// <returns></returns>
    [HttpGet("tree")]
    public async Task<GetFullCategoryTreeResponseModel> GetCategoryTree()
    {
        var model = new GetFullCategoryTreeRequestModel();
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Get Categories
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<GetCategoriesResponseModel> GetCategories([FromQuery] GetCategoriesRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Get Category By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<GetCategoryByIdResponseModel> GetCategoryById([FromRoute] string id)
    {
        var model =  new GetCategoryByIdRequestModel();
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Create Category
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<CreateCategoryResponseModel> CreateCategory([FromBody] CreateCategoryRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Update Category
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<UpdateCategoryResponseModel> UpdateCategory([FromRoute] string id, [FromBody] UpdateCategoryRequestModel model)
    {
        model.Id = id;
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Delete Category
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<DeleteCategoryResponseModel> DeleteCategory([FromRoute] string id)
    {
        return await Mediator.Send(new DeleteCategoryRequestModel { Id = id });
    }
    

    
}
