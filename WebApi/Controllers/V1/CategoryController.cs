using Application.Services.Categories.Commands;
using Application.Services.Categories.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/users")]
public class CategoryController : BaseController
{
    /// <summary>
    /// Get Category Tree
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("tree")]
    public async Task<GetFullCategoryTreeResponseModel> GetUsers()
    {
        var model = new GetFullCategoryTreeRequestModel();
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Create Category
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<CreateCategoryResponseModel> GetUsers([FromBody] CreateCategoryRequestModel model)
    {
        return await Mediator.Send(model);
    }
    

    
}
