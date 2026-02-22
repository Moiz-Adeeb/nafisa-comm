using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.CompanyRoles.Models;
using Application.Shared;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyRoles.Queries;

public class GetCompanyRolesRequestModel : GetPagedRequest<GetCompanyRolesResponseModel>
{
    public string Search { get; set; }
    public bool? IsActive { get; set; }
}

public class GetCompanyRolesRequestModelValidator : AbstractValidator<GetCompanyRolesRequestModel>
{
    public GetCompanyRolesRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetCompanyRolesRequestHandler
    : IRequestHandler<GetCompanyRolesRequestModel, GetCompanyRolesResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyRolesRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyRolesResponseModel> Handle(
        GetCompanyRolesRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();
        Expression<Func<CompanyRole, bool>> query = p => p.CompanyId == companyId;
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

        var totalRecords = await _context.CompanyRoles.ActiveCount(cancellationToken);

        var roles = await _context
            .CompanyRoles.GetManyReadOnly(query, request)
            .Select(CompanyRoleSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetCompanyRolesResponseModel { Data = roles, Count = totalRecords };
    }
}

public class GetCompanyRolesResponseModel
{
    public List<CompanyRoleDto> Data { get; set; }
    public int Count { get; set; }
}
