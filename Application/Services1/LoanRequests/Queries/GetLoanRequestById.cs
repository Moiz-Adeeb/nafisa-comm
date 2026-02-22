using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.LoanRequests.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanRequests.Queries;

public class GetLoanRequestByIdRequestModel : IRequest<GetLoanRequestByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetLoanRequestByIdRequestModelValidator
    : AbstractValidator<GetLoanRequestByIdRequestModel>
{
    public GetLoanRequestByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetLoanRequestByIdRequestHandler
    : IRequestHandler<GetLoanRequestByIdRequestModel, GetLoanRequestByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetLoanRequestByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetLoanRequestByIdResponseModel> Handle(
        GetLoanRequestByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var loanRequest = await _context.LoanRequests.GetByWithSelectAsync(
            r => r.Id == request.Id && r.CompanyId == companyId,
            LoanRequestSelector.Selector,
            cancellationToken: cancellationToken
        );

        if (loanRequest == null)
        {
            throw new NotFoundException("Loan request not found");
        }

        return new GetLoanRequestByIdResponseModel { Data = loanRequest };
    }
}

public class GetLoanRequestByIdResponseModel
{
    public LoanRequestDto Data { get; set; }
}
