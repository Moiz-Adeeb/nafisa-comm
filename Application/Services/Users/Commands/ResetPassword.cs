using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class ResetPasswordRequestModel : IRequest<ResetPasswordResponseModel>
{
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string Token { get; set; }
    public string Email { get; set; }
}

public class ResetPasswordRequestModelValidator : AbstractValidator<ResetPasswordRequestModel>
{
    public ResetPasswordRequestModelValidator()
    {
        RuleFor(p => p.Password).Required().Password();
        RuleFor(p => p.ConfirmPassword).Required().Equal(p => p.Password);
        RuleFor(p => p.Token).Required();
        RuleFor(p => p.Email).Required().Max(50);
    }
}

public class ResetPasswordRequestHandler : IRequestHandler<ResetPasswordRequestModel, ResetPasswordResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly UserManager<User> _userManager;

    public ResetPasswordRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        UserManager<User> userManager
    )
    {
        _context = context;
        _sessionService = sessionService;
        _userManager = userManager;
    }

    public async Task<ResetPasswordResponseModel> Handle(
        ResetPasswordRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower().Trim();
        var user = await _context.Users.GetByAsync(
            p => p.Email == email,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            throw new NotFoundException(nameof(user));
        }
        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.ConfirmPassword
        );
        if (result.Succeeded) return new ResetPasswordResponseModel();
        
        throw new BadRequestException(result.Errors.First().Description);
    }
}

public class ResetPasswordResponseModel
{
    public ResetPasswordResponseModel() { }
}