using System.Security.Claims;
using Application.Exceptions;
using Application.Interfaces;
using Common.Constants;
using Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Persistence.Context;
using Persistence.Extension;
using System.Linq;
using Application.Services.Users.Commands;
using MediatR;

namespace WebApi.Controllers.V1;

public class SignUpController : BaseController
{

    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public SignUpController(
        UserManager<User> userManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Register new User 
    /// </summary>
    /// <returns>Confirmation Message</returns>
    /// <exception cref="BadRequestException"></exception>
    [HttpPost("~/signup")]
    [Produces("application/json")]
    public async Task<RegisterUserResponseModel> RegisterUser([FromBody] RegisterUserRequestModel model)
    {
        return await Mediator.Send(model);
    }  

    //public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto request) 
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return BadRequest(ModelState);
    //    }

    //    var normalizedUsername = request.Username?.ToLower().Trim();

    //    var existCheck = await _userManager.FindByNameAsync(normalizedUsername);
    //    if (existCheck != null)
    //    {
    //        throw new BadRequestException("The Username is not available")
    //    }

    //    var newUser = new Users
    //    {
    //        UserName = request.Username,
    //        Name = request.Username,
    //        EmailConfirmed = false,
    //        PasswordHash = new PasswordHasher<Users>().HashPassword(request.password),
    //    };
    //}  
    
}
