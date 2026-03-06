using System.Linq.Expressions;
using Application.Services.Users.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Users.Queries;

public class GetUsersRequestModel : GetPagedRequest<GetUsersResponseModel>{  }

public class GetUsersRequestModelValidator : PageRequestValidator<GetUsersRequestModel>
{ public GetUsersRequestModelValidator() {  } }

public class GetUsersRequestHandler : IRequestHandler<GetUsersRequestModel, GetUsersResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetUsersRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetUsersResponseModel> Handle(
        GetUsersRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<User, bool>> query = p => true;
        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.UserName.Contains(request.Search) || p.Name.ToLower().Contains(request.Search)
            );
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
