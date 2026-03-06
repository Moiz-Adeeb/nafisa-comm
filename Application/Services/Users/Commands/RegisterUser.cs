using Application.Exceptions;
using Application.Extensions;
using Application.Services.Users.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class RegisterUserRequestModel : IRequest<RegisterUserResponseModel>
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class RegisterUserRequestModelValidator : AbstractValidator<RegisterUserRequestModel>
{
    public RegisterUserRequestModelValidator()
    {
        RuleFor(x => x.Email).Required().EmailAddress();
        RuleFor(x => x.Name).Required().Max(50);
        RuleFor(x => x.PhoneNumber).Required().Phone();
        RuleFor(x => x.Password).Password().Max(50);
        RuleFor(x => x.ConfirmPassword).Matches(p => p.Password);
    }
}

public class RegisterUserRequestHandler : IRequestHandler<RegisterUserRequestModel, RegisterUserResponseModel>
{
    private readonly ApplicationDbContext _context;

    public RegisterUserRequestHandler (ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<RegisterUserResponseModel> Handle(
        RegisterUserRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var username = request.Email;
        var existCheck = await _context.Users
            .ActiveAny(u => u.UserName == username, cancellationToken);
        
        if (existCheck) throw new AlreadyExistsException(nameof(username));

        var user = new User()
        {
            Email = request.Email,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber
        }; 
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new RegisterUserResponseModel() { Data = new UserDto(user) };
    }
}

public class RegisterUserResponseModel
{
    public UserDto Data { get; set; }
}
