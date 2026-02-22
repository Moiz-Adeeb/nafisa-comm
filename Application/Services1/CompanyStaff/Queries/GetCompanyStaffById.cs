using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.CompanyStaff.Models;
using Domain.Constant;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyStaff.Queries;

public class GetCompanyStaffByIdRequestModel : IRequest<GetCompanyStaffByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetCompanyStaffByIdRequestModelValidator
    : AbstractValidator<GetCompanyStaffByIdRequestModel>
{
    public GetCompanyStaffByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetCompanyStaffByIdRequestHandler
    : IRequestHandler<GetCompanyStaffByIdRequestModel, GetCompanyStaffByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyStaffByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyStaffByIdResponseModel> Handle(
        GetCompanyStaffByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var companyStaff = await _context.Users.GetByWithSelectAsync(
            p => p.Id == request.Id && p.CompanyStaff.CompanyId == companyId,
            CompanyStaffSelector.Selector,
            cancellationToken: cancellationToken
        );

        if (companyStaff == null)
        {
            throw new NotFoundException("Company staff not found");
        }

        return new GetCompanyStaffByIdResponseModel { Data = companyStaff };
    }
}

public class GetCompanyStaffByIdResponseModel
{
    public CompanyStaffDto Data { get; set; }
}
