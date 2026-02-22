using Application.Interfaces;
using Application.Services.CompanyBranches.Models;
using Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyBranches.Queries;

public class GetCompanyBranchesDropDownRequestModel
    : IRequest<GetCompanyBranchesDropDownResponseModel> { }

public class GetCompanyBranchesDropDownRequestHandler
    : IRequestHandler<
        GetCompanyBranchesDropDownRequestModel,
        GetCompanyBranchesDropDownResponseModel
    >
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyBranchesDropDownRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyBranchesDropDownResponseModel> Handle(
        GetCompanyBranchesDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var branches = await _context
            .CompanyBranches.Where(b => !b.IsDeleted && b.CompanyId == companyId)
            .OrderBy(b => b.Name)
            .Select(CompanyBranchSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetCompanyBranchesDropDownResponseModel { Data = branches };
    }
}

public class GetCompanyBranchesDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
