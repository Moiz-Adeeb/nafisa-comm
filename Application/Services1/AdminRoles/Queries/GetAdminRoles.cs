using System.Linq.Expressions;
using Application.Extensions;
using Application.Services.AdminRoles.Models;
using Application.Shared;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AdminRoles.Queries;

public class GetAdminRolesRequestModel : GetPagedRequest<GetAdminRolesResponseModel>
{
    public bool? IsActive { get; set; }
}

public class GetAdminRolesRequestModelValidator
    : PageRequestValidator<GetAdminRolesRequestModel> { }

public class GetAdminRolesRequestHandler
    : IRequestHandler<GetAdminRolesRequestModel, GetAdminRolesResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetAdminRolesRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdminRolesResponseModel> Handle(
        GetAdminRolesRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<AdminRole, bool>> query = p => true;

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.AndAlso(r =>
                r.Name.ToLower().Contains(search)
                || (r.Description != null && r.Description.ToLower().Contains(search))
            );
        }

        // Apply IsActive filter
        if (request.IsActive.HasValue)
        {
            query = query.AndAlso(r => r.IsActive == request.IsActive.Value);
        }

        var totalRecords = await _context.AdminRoles.ActiveCount(cancellationToken);

        var roles = await _context
            .AdminRoles.GetManyReadOnly(query, request)
            .Select(AdminRoleSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetAdminRolesResponseModel { Data = roles, Count = totalRecords };
    }
}

public class GetAdminRolesResponseModel
{
    public List<AdminRoleDto> Data { get; set; }
    public int Count { get; set; }
}
