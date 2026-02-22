using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.CompanyStaff.Models;
using Application.Shared;
using Domain.Constant;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyStaff.Queries;

public class GetCompanyStaffsRequestModel : GetPagedRequest<GetCompanyStaffsResponseModel>
{
    public string Search { get; set; }
    public bool? IsEnabled { get; set; }
    public string CompanyRoleId { get; set; }
    public string RoleName { get; set; } // Filter by CompanyAdmin or Employee
    // public string BranchId { get; set; } // If entity supports BranchId
}

public class GetCompanyStaffsRequestModelValidator : AbstractValidator<GetCompanyStaffsRequestModel>
{
    public GetCompanyStaffsRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetCompanyStaffsRequestHandler
    : IRequestHandler<GetCompanyStaffsRequestModel, GetCompanyStaffsResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyStaffsRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyStaffsResponseModel> Handle(
        GetCompanyStaffsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        // Build query to get only company staff (users with CompanyAdmin or Employee role)
        Expression<Func<User, bool>> query = p => p.CompanyStaff.CompanyId == companyId;

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

        // Apply CompanyRoleId filter
        if (!string.IsNullOrWhiteSpace(request.CompanyRoleId))
        {
            query = query.AndAlso(u => u.CompanyStaff.CompanyRoleId == request.CompanyRoleId);
        }

        // Apply RoleName filter
        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            query = query.AndAlso(u => u.UserRoles.Any(ur => ur.Role.Name == request.RoleName));
        }

        // Apply BranchId filter (if entity supports BranchId)
        // if (!string.IsNullOrWhiteSpace(request.BranchId))
        // {
        //     query = query.AndAlso(u => u.BranchId == request.BranchId);
        // }

        var totalRecords = await _context
            .Users.Where(query)
            .Where(u => !u.IsDeleted)
            .CountAsync(cancellationToken);

        var companyStaffs = await _context
            .Users.GetManyReadOnly(null, request)
            .Select(CompanyStaffSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetCompanyStaffsResponseModel { Data = companyStaffs, Count = totalRecords };
    }
}

public class GetCompanyStaffsResponseModel
{
    public List<CompanyStaffDto> Data { get; set; }
    public int Count { get; set; }
}
