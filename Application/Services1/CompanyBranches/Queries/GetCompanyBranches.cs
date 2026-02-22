using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.CompanyBranches.Models;
using Application.Shared;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyBranches.Queries;

public class GetCompanyBranchesRequestModel : GetPagedRequest<GetCompanyBranchesResponseModel>
{
    public string Search { get; set; }
}

public class GetCompanyBranchesRequestModelValidator
    : AbstractValidator<GetCompanyBranchesRequestModel>
{
    public GetCompanyBranchesRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetCompanyBranchesRequestHandler
    : IRequestHandler<GetCompanyBranchesRequestModel, GetCompanyBranchesResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyBranchesRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyBranchesResponseModel> Handle(
        GetCompanyBranchesRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        Expression<Func<CompanyBranch, bool>> query = b => b.CompanyId == companyId;

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.AndAlso(b =>
                b.Name.ToLower().Contains(search)
                || b.Code.ToLower().Contains(search)
                || (b.Address != null && b.Address.ToLower().Contains(search))
                || (b.PhoneNumber != null && b.PhoneNumber.Contains(search))
            );
        }

        var totalRecords = await _context
            .CompanyBranches.Where(query)
            .Where(b => !b.IsDeleted)
            .CountAsync(cancellationToken);

        var branches = await _context
            .CompanyBranches.GetManyReadOnly(null, request)
            .Select(CompanyBranchSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetCompanyBranchesResponseModel { Data = branches, Count = totalRecords };
    }
}

public class GetCompanyBranchesResponseModel
{
    public List<CompanyBranchDto> Data { get; set; }
    public int Count { get; set; }
}
