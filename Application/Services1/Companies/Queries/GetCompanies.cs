using System.Linq.Expressions;
using Application.Extensions;
using Application.Services.Companies.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Companies.Queries;

public class GetCompaniesRequestModel : GetPagedRequest<GetCompaniesResponseModel>
{
    public RequestStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? SubscriptionActive { get; set; }
}

public class GetCompaniesRequestModelValidator : PageRequestValidator<GetCompaniesRequestModel> { }

public class GetCompaniesRequestHandler
    : IRequestHandler<GetCompaniesRequestModel, GetCompaniesResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetCompaniesRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetCompaniesResponseModel> Handle(
        GetCompaniesRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<Company, bool>> query = p => true;

        if (request.Search.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p =>
                p.FirstName.ToLower().Contains(request.Search.ToLower())
                || p.LastName.ToLower().Contains(request.Search.ToLower())
                || p.Email.ToLower().Contains(request.Search.ToLower())
                || p.CompanyName.ToLower().Contains(request.Search.ToLower())
            );
        }

        if (request.Status.HasValue)
        {
            query = query.AndAlso(p => p.Status == request.Status);
        }

        if (request.IsActive.HasValue)
        {
            query = query.AndAlso(p => p.IsActive == request.IsActive);
        }

        if (request.SubscriptionActive.HasValue)
        {
            query = query.AndAlso(p => p.SubscriptionActive == request.SubscriptionActive);
        }

        var list = await _context
            .Companies.GetManyReadOnly(query, request)
            .Select(CompanySelector.Selector)
            .ToListAsync(cancellationToken);

        var count = await _context.Companies.ActiveCount(query, cancellationToken);

        return new GetCompaniesResponseModel { Data = list, Count = count };
    }
}

public class GetCompaniesResponseModel
{
    public List<CompanyDto> Data { get; set; }
    public int Count { get; set; }
}
