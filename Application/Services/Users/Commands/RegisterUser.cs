using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Users.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;
using System.Security.Cryptography.X509Certificates;

namespace Application.Services.Users.Commands;

public class RegisterUserRequestModel : IRequest<RegisterUserResponseModel>
{
    public string UserName { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class RegisterUserRequestModelValidator : AbstractValidator<RegisterUserRequestModel>
{
    public RegisterUserRequestModelValidator()
    {
        RuleFor(x => x.Name).Required().Max(50);
        RuleFor(x => x.UserName).Required().Max(16);
        RuleFor(x => x.Password).Password().Max(50);
        RuleFor(x => x.ConfirmPassword).Matches(p => p.Password);
    }
}

public class RegisterUserRequestHandler
    : IRequestHandler<RegisterUserRequestModel, RegisterUserResponseModel>
{
    private readonly ApplicationDbContext _context;

    public RegisterUserRequestHandler (
        ApplicationDbContext context
    )
    {
        _context = context;
    }
    public async Task<RegisterUserResponseModel> Handle(
        RegisterUserRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var username = request.UserName;
        var existCheck = await _context.Users.ActiveAny(u => u.UserName == username);
        if (existCheck)
        {
            throw new AlreadyExistsException(nameof(username));
        }

        var user = new User()
        {
            ChatId = Guid.NewGuid().ToString(),
            UserName = request.UserName,
            Name = request.Name,
            Status = request.Status
        }; 
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterUserResponseModel() {  };
    }
}

public class RegisterUserResponseModel
{
    public UsersDto Data { get; set; }
}
