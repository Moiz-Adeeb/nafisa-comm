using Application.Services.Users.Commands;
using Application.Services.Users.Queries;
using Common.Extensions;
using Domain.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Shared;

namespace WebApi.Controllers.V1;

[Route("api/v1/users")]
public class UserController : BaseController
{
    /// <summary>
    /// Get Users
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<GetUsersResponseModel> GetUsers([FromQuery] GetUsersRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Change Current Password
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("password")]
    public async Task<ChangePasswordResponseModel> ChangeCurrentPassword(ChangePasswordRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Update Password By Id of user
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPost("password/{id}")]
    public async Task<UpdatePasswordByIdResponseModel> UpdatePasswordById([FromRoute] string id, UpdatePasswordByIdRequestModel model)
    {
        model.Id = id;
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Get User Profile
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<GetUserProfileResponseModel> GetUserProfile()
    {
        var model = new GetUserProfileRequestModel();
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Get Users Drop Down
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("dropDown")]
    public async Task<GetUsersDropDownResponseModel> GetUsersDropDown([FromQuery] GetUsersDropDownRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Get User By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<GetUserByIdResponseModel> GetUserById([FromRoute] string id)
    {
        var model = new GetUserByIdRequestModel() { Id = id };
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Create User
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<CreateUserResponseModel> CreateUser([FromBody] CreateUserRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Create User with Link
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("link")]
    public async Task<CreateUserWithLinkResponseModel> CreateUserWithLink([FromBody] CreateUserWithLinkRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Update User Profile
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("me")]
    public async Task<UpdateUserProfileResponseModel> UpdateUserProfile([FromBody] UpdateUserProfileRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Update User
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<UpdateUserResponseModel> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequestModel model)
    {
        model.Id = id;
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Delete User
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<DeleteUserResponseModel> DeleteUser([FromRoute] string id)
    {
        return await Mediator.Send(new DeleteUserRequestModel { Id = id });
    }

    /// <summary>
    /// Reset Password
    /// </summary>
    /// <returns></returns>
    [HttpPost("reset-password")]
    public async Task<ResetPasswordResponseModel> ResetPassword([FromBody] ResetPasswordRequestModel model)
    {
        return await Mediator.Send(model);
    }

    /// <summary>
    /// Generate Reset Token
    /// </summary>
    /// <returns></returns>
    [HttpPost("reset-token")]
    public async Task<GenerateResetPasswordTokenResponseModel> GenerateResetPassword(GenerateResetPasswordTokenRequestModel model)
    {
        return await Mediator.Send(model);
    }
    
    /// <summary>
    /// Checks if User is active or the company it belongs to is active
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("isActive")]
    public async Task<IsUserActiveResponseModel> IsUserActive()
    {
        var model = new IsUserActiveRequestModel();
        return await Mediator.Send(model);
    }
}

///// <summary>
///// Create User Bulk
///// </summary>
///// <param name="model"></param>
///// <returns></returns>
//[Authorize]
//[HttpPost("bulk")]
//public async Task<CreateUserBulkResponseModel> CreateUserBulk(
//    [FromBody] CreateUserBulkRequestModel model
//)
//{
//    return await Mediator.Send(model);
//}

///// <summary>
///// Import User
///// </summary>
///// <param name="file"></param>
///// <returns></returns>
//[Authorize]
//[HttpPost("import")]
//public async Task<ImportUserBulkResponseModel> ImportDocker([FromForm] IFormFile[] file)
//{
//    var model = new ImportUserBulkRequestModel()
//    {
//        Host = Request.Host.Host,
//        Stream = new MemoryStream(
//            Convert.FromBase64String(file[0].OpenReadStream().StreamToBase64())
//        ),
//    };
//    return await Mediator.Send(model);
//}
