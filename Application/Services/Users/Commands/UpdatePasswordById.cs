using System.Linq.Expressions;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Users.Commands;

public class UpdatePasswordByIdRequestModel : IRequest<UpdatePasswordByIdResponseModel>
{
    public string Id { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class UpdatePasswordByIdRequestModelValidator
    : AbstractValidator<UpdatePasswordByIdRequestModel>
{
    public UpdatePasswordByIdRequestModelValidator()
    {
        RuleFor(p => p.Password).Required().Password();
        RuleFor(p => p.ConfirmPassword).Required().Matches(p => p.Password);
    }
}

public class UpdatePasswordByIdRequestHandler
    : IRequestHandler<UpdatePasswordByIdRequestModel, UpdatePasswordByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public UpdatePasswordByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<UpdatePasswordByIdResponseModel> Handle(
        UpdatePasswordByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<User, bool>> query = p => p.Id == request.Id;

        var user = await _context.Users.FirstOrDefaultAsync(
            query,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            throw new NotFoundException(nameof(user));
        }
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return new UpdatePasswordByIdResponseModel();
    }
}

public class UpdatePasswordByIdResponseModel { }