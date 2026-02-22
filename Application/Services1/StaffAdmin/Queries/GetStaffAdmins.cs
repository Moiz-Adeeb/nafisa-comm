using System.Linq.Expressions;
using Application.Extensions;
using Application.Services.StaffAdmin.Models;
using Application.Shared;
using Domain.Constant;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.StaffAdmin.Queries;

public class GetStaffAdminsRequestModel : GetPagedRequest<GetStaffAdminsResponseModel>
{
    public string Search { get; set; }
    public bool? IsEnabled { get; set; }
    public string AdminRoleId { get; set; }
}

public class GetStaffAdminsRequestModelValidator : AbstractValidator<GetStaffAdminsRequestModel>
{
    public GetStaffAdminsRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetStaffAdminsRequestHandler
    : IRequestHandler<GetStaffAdminsRequestModel, GetStaffAdminsResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetStaffAdminsRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetStaffAdminsResponseModel> Handle(
        GetStaffAdminsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        // Build query to get only admin staff (users with Administrator role)
        Expression<Func<User, bool>> query = u =>
            u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Administrator);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.AndAlso(u =>
                u.FullName.ToLower().Contains(search)
                || u.Email.ToLower().Contains(search)
                || (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
            );
        }

        // Apply IsEnabled filter
        if (request.IsEnabled.HasValue)
        {
            query = query.AndAlso(u => u.IsEnabled == request.IsEnabled.Value);
        }

        // Apply AdminRoleId filter
        if (!string.IsNullOrWhiteSpace(request.AdminRoleId))
        {
            query = query.AndAlso(u => u.AdminRoleId == request.AdminRoleId);
        }

        var totalRecords = await _context
            .Users.Where(query)
            .Where(u => !u.IsDeleted)
            .CountAsync(cancellationToken);

        var staffAdmins = await _context
            .Users.GetManyReadOnly(query, request)
            .Select(StaffAdminSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetStaffAdminsResponseModel { Data = staffAdmins, Count = totalRecords };
    }
}

public class GetStaffAdminsResponseModel
{
    public List<StaffAdminDto> Data { get; set; }
    public int Count { get; set; }
}
