using System.Linq.Expressions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.LoanOffers.Models;
using Application.Shared;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanOffers.Queries;

public class GetLoanOffersRequestModel : GetPagedRequest<GetLoanOffersResponseModel>
{
    public string Search { get; set; }
    public bool? IsActive { get; set; }
}

public class GetLoanOffersRequestModelValidator : AbstractValidator<GetLoanOffersRequestModel>
{
    public GetLoanOffersRequestModelValidator()
    {
        RuleFor(p => p.Page).GreaterThanOrEqualTo(1);
        RuleFor(p => p.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100);
    }
}

public class GetLoanOffersRequestHandler
    : IRequestHandler<GetLoanOffersRequestModel, GetLoanOffersResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetLoanOffersRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetLoanOffersResponseModel> Handle(
        GetLoanOffersRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        Expression<Func<LoanOffer, bool>> query = o => o.CompanyId == companyId;

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.AndAlso(o =>
                o.Title.ToLower().Contains(search)
                || (o.Description != null && o.Description.ToLower().Contains(search))
            );
        }

        // Apply IsActive filter
        if (request.IsActive.HasValue)
        {
            query = query.AndAlso(o => o.IsActive == request.IsActive.Value);
        }

        var totalRecords = await _context
            .LoanOffers.Where(query)
            .Where(o => !o.IsDeleted)
            .CountAsync(cancellationToken);

        var loanOffers = await _context
            .LoanOffers.GetManyReadOnly(null, request)
            .Select(LoanOfferSelector.Selector)
            .ToListAsync(cancellationToken);

        // Parse durations for each offer
        foreach (var offer in loanOffers)
        {
            var offerEntity = await _context
                .LoanOffers.Where(o => o.Id == offer.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (offerEntity != null && !string.IsNullOrWhiteSpace(offerEntity.Durations))
            {
                offer.Durations = offerEntity
                    .Durations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => int.TryParse(d.Trim(), out var duration) ? duration : 0)
                    .Where(d => d > 0)
                    .ToList();
            }
        }

        return new GetLoanOffersResponseModel { Data = loanOffers, Count = totalRecords };
    }
}

public class GetLoanOffersResponseModel
{
    public List<LoanOfferDto> Data { get; set; }
    public int Count { get; set; }
}
