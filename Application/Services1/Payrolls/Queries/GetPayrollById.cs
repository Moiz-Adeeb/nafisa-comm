using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Payrolls.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Payrolls.Queries;

public class GetPayrollByIdRequestModel : IRequest<GetPayrollByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetPayrollByIdRequestModelValidator : AbstractValidator<GetPayrollByIdRequestModel>
{
    public GetPayrollByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetPayrollByIdRequestHandler
    : IRequestHandler<GetPayrollByIdRequestModel, GetPayrollByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetPayrollByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetPayrollByIdResponseModel> Handle(
        GetPayrollByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var payroll = await _context.PayRolls.GetByWithSelectAsync(
            p => p.Id == request.Id && p.CompanyId == companyId,
            PayrollSelector.Selector,
            cancellationToken: cancellationToken
        );

        if (payroll == null)
        {
            throw new NotFoundException("Payroll not found");
        }

        return new GetPayrollByIdResponseModel { Data = payroll };
    }
}

public class GetPayrollByIdResponseModel
{
    public PayrollDto Data { get; set; }
}