using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.CompanyBranches.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyBranches.Queries;

public class GetCompanyBranchByIdRequestModel : IRequest<GetCompanyBranchByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetCompanyBranchByIdRequestModelValidator
    : AbstractValidator<GetCompanyBranchByIdRequestModel>
{
    public GetCompanyBranchByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetCompanyBranchByIdRequestHandler
    : IRequestHandler<GetCompanyBranchByIdRequestModel, GetCompanyBranchByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyBranchByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyBranchByIdResponseModel> Handle(
        GetCompanyBranchByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var branch = await _context
            .CompanyBranches.Include(b => b.Company)
            .Include(b => b.CompanyStaffs)
            .Where(b => b.Id == request.Id && !b.IsDeleted && b.CompanyId == companyId)
            .Select(CompanyBranchSelector.Selector)
            .FirstOrDefaultAsync(cancellationToken);

        if (branch == null)
        {
            throw new NotFoundException("Branch not found");
        }

        return new GetCompanyBranchByIdResponseModel { Data = branch };
    }
}

public class GetCompanyBranchByIdResponseModel
{
    public CompanyBranchDto Data { get; set; }
}
