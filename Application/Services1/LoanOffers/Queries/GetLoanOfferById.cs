using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.LoanOffers.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.LoanOffers.Queries;

public class GetLoanOfferByIdRequestModel : IRequest<GetLoanOfferByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetLoanOfferByIdRequestModelValidator : AbstractValidator<GetLoanOfferByIdRequestModel>
{
    public GetLoanOfferByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetLoanOfferByIdRequestHandler
    : IRequestHandler<GetLoanOfferByIdRequestModel, GetLoanOfferByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetLoanOfferByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetLoanOfferByIdResponseModel> Handle(
        GetLoanOfferByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var loanOffer = await _context
            .LoanOffers.Include(o => o.Company)
            .Include(o => o.LoanRequests)
            .Where(o => o.Id == request.Id && !o.IsDeleted && o.CompanyId == companyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (loanOffer == null)
        {
            throw new NotFoundException("Loan offer not found");
        }

        var dto = new LoanOfferDto(loanOffer);

        return new GetLoanOfferByIdResponseModel { Data = dto };
    }
}

public class GetLoanOfferByIdResponseModel
{
    public LoanOfferDto Data { get; set; }
}
