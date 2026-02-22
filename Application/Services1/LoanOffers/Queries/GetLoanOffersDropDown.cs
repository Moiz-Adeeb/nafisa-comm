using Application.Interfaces;
using Application.Services.LoanOffers.Models;
using Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.LoanOffers.Queries;

public class GetLoanOffersDropDownRequestModel : IRequest<GetLoanOffersDropDownResponseModel> { }

public class GetLoanOffersDropDownRequestHandler
    : IRequestHandler<GetLoanOffersDropDownRequestModel, GetLoanOffersDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetLoanOffersDropDownRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetLoanOffersDropDownResponseModel> Handle(
        GetLoanOffersDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var loanOffers = await _context
            .LoanOffers.Where(o => !o.IsDeleted && o.IsActive && o.CompanyId == companyId)
            .OrderBy(o => o.Title)
            .Select(LoanOfferSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetLoanOffersDropDownResponseModel { Data = loanOffers };
    }
}

public class GetLoanOffersDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
