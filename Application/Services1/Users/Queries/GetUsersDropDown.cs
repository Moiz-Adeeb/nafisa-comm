using System.Linq.Expressions;
using Application.Interfaces;
using Application.Services.Users.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Queries;

public class GetUsersDropDownRequestModel : IRequest<GetUsersDropDownResponseModel>
{
    public string Search { get; set; }
    public int? Limit { get; set; }
    public string Role { get; set; }
}

public class GetUsersDropDownRequestModelValidator : AbstractValidator<GetUsersDropDownRequestModel>
{
    public GetUsersDropDownRequestModelValidator() { }
}

public class GetUsersDropDownRequestHandler
    : IRequestHandler<GetUsersDropDownRequestModel, GetUsersDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetUsersDropDownRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetUsersDropDownResponseModel> Handle(
        GetUsersDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<User, bool>> query = p => true;
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.Email.ToLower().Contains(request.Search)
                || p.FullName.ToLower().Contains(request.Search)
            );
        }

        if (request.Role.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso<User>(p => p.UserRoles.Any(x => x.Role.Name == request.Role));
        }
        var queryable = _context.Users.GetAllReadOnly(query);
        if (request.Limit.HasValue)
        {
            queryable = queryable.Take(request.Limit.Value);
        }
        var list = await queryable
            .Select(UserSelector.SelectorDropDown)
            .ToListAsync(cancellationToken: cancellationToken);
        return new GetUsersDropDownResponseModel() { Data = list };
    }
}

public class GetUsersDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
