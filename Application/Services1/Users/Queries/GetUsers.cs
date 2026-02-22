using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Users.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Constant;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Queries;

public class GetUsersRequestModel : GetPagedRequest<GetUsersResponseModel>
{
    public string Role { get; set; }

    public bool? IsEnabled { get; set; }
}

public class GetUsersRequestModelValidator : PageRequestValidator<GetUsersRequestModel>
{
    public GetUsersRequestModelValidator()
    {
        When(
            p => p.Role != null,
            () =>
            {
                RuleFor(p => p.Role).MustBeOneOf(RoleNames.AllRoles);
            }
        );
    }
}

public class GetUsersRequestHandler : IRequestHandler<GetUsersRequestModel, GetUsersResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetUsersRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetUsersResponseModel> Handle(
        GetUsersRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<User, bool>> query = p => true;
        if (request.Role != null)
        {
            query = p => p.UserRoles.Any(x => x.Role.Name == request.Role);
        }
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.Email.Contains(request.Search) || p.FullName.ToLower().Contains(request.Search)
            );
        }
        if (request.IsEnabled.HasValue)
        {
            query = query.AndAlso(p => p.IsEnabled == request.IsEnabled);
        }
        var list = await _context
            .Users.GetManyReadOnly(query, request)
            .Select(UserSelector.Selector)
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.Users.ActiveCount(query, cancellationToken: cancellationToken);
        return new GetUsersResponseModel() { Data = list, Count = count };
    }
}

public class GetUsersResponseModel
{
    public List<UserDto> Data { get; set; }
    public int Count { get; set; }
}
