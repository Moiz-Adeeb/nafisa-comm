using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Payrolls.Models;
using Application.Shared;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Payrolls.Queries;

public class GetPayrollsRequestModel : GetPagedRequest<GetPayrollsResponseModel>
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string BranchId { get; set; }
    public string CompanyStaffId { get; set; }
    public string Search { get; set; }
}

public class GetPayrollsRequestModelValidator : AbstractValidator<GetPayrollsRequestModel>
{
    public GetPayrollsRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
        RuleFor(p => p.Month).InclusiveBetween(1, 12).When(p => p.Month.HasValue);
        RuleFor(p => p.Year).GreaterThan(2000).LessThanOrEqualTo(2100).When(p => p.Year.HasValue);
    }
}

public class GetPayrollsRequestHandler
    : IRequestHandler<GetPayrollsRequestModel, GetPayrollsResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetPayrollsRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetPayrollsResponseModel> Handle(
        GetPayrollsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        // Build query
        Expression<Func<PayRoll, bool>> query = p => p.CompanyId == companyId;

        // Apply month filter
        if (request.Month.HasValue)
        {
            query = query.AndAlso(p => p.Month == request.Month.Value);
        }

        // Apply year filter
        if (request.Year.HasValue)
        {
            query = query.AndAlso(p => p.Year == request.Year.Value);
        }

        // Apply branch filter
        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            query = query.AndAlso(p => p.CompanyStaff.BranchId == request.BranchId);
        }

        // Apply company staff filter
        if (!string.IsNullOrWhiteSpace(request.CompanyStaffId))
        {
            query = query.AndAlso(p => p.CompanyStaffId == request.CompanyStaffId);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.AndAlso(p =>
                p.CompanyStaff.User.FullName.ToLower().Contains(search)
                || p.CompanyStaff.User.Email.ToLower().Contains(search)
            );
        }

        var totalRecords = await _context
            .PayRolls.Where(query)
            .CountAsync(cancellationToken);

        var payrolls = await _context
            .PayRolls.Where(query)
            .Include(p => p.CompanyStaff)
            .ThenInclude(cs => cs.User)
            .Include(p => p.CompanyStaff)
            .ThenInclude(cs => cs.Branch)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ThenBy(p => p.CompanyStaff.User.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(PayrollSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetPayrollsResponseModel { Data = payrolls, Count = totalRecords };
    }
}

public class GetPayrollsResponseModel
{
    public List<PayrollDto> Data { get; set; }
    public int Count { get; set; }
}