using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.LoanRequests.Models;
using Application.Shared;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanRequests.Queries;

public class GetLoanRequestsRequestModel : GetPagedRequest<GetLoanRequestsResponseModel>
{
    public LoanRequestStatus? Status { get; set; }
    public string CompanyStaffId { get; set; }
}

public class GetLoanRequestsRequestModelValidator : AbstractValidator<GetLoanRequestsRequestModel>
{
    public GetLoanRequestsRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetLoanRequestsRequestHandler
    : IRequestHandler<GetLoanRequestsRequestModel, GetLoanRequestsResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetLoanRequestsRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetLoanRequestsResponseModel> Handle(
        GetLoanRequestsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        Expression<Func<LoanRequest, bool>> query = r => r.CompanyId == companyId;

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.AndAlso(r =>
                r.Title.ToLower().Contains(search)
                || (r.CompanyStaff.User.FullName.ToLower().Contains(search))
                || (r.Purpose != null && r.Purpose.ToLower().Contains(search))
            );
        }

        // Apply Status filter
        if (request.Status.HasValue)
        {
            query = query.AndAlso(r => r.Status == request.Status.Value);
        }

        // Apply EmployeeId filter
        if (!string.IsNullOrWhiteSpace(request.CompanyStaffId))
        {
            query = query.AndAlso(r => r.CompanyStaffId == request.CompanyStaffId);
        }

        var totalRecords = await _context
            .LoanRequests.Where(query)
            .Where(r => !r.IsDeleted)
            .CountAsync(cancellationToken);

        var loanRequests = await _context
            .LoanRequests.GetManyReadOnly(null, request)
            .Select(LoanRequestSelector.Selector)
            .ToListAsync(cancellationToken);

        return new GetLoanRequestsResponseModel { Data = loanRequests, Count = totalRecords };
    }
}

public class GetLoanRequestsResponseModel
{
    public List<LoanRequestDto> Data { get; set; }
    public int Count { get; set; }
}
