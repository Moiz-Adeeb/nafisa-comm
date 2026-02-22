using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Commands;

public class ChangePasswordRequestModel : IRequest<ChangePasswordResponseModel>
{
    public string CurrentPassword { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class ChangePasswordRequestModelValidator : AbstractValidator<ChangePasswordRequestModel>
{
    public ChangePasswordRequestModelValidator()
    {
        RuleFor(p => p.Password).Required().Min(6);
        RuleFor(p => p.ConfirmPassword).Required().Matches(p => p.Password);
    }
}

public class ChangePasswordRequestHandler
    : IRequestHandler<ChangePasswordRequestModel, ChangePasswordResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public ChangePasswordRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<ChangePasswordResponseModel> Handle(
        ChangePasswordRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var user = await _context.Users.GetByAsync(
            p => p.Id == userId,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            throw new NotFoundException(nameof(user));
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new BadRequestException("Invalid Current Password");
        }

        user.PasswordHash = hasher.HashPassword(user, request.Password);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return new ChangePasswordResponseModel();
    }
}

public class ChangePasswordResponseModel { }
