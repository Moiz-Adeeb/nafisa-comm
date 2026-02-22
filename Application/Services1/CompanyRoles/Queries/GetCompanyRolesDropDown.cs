using Application.Interfaces;
using Application.Services.CompanyRoles.Models;
using Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyRoles.Queries;

public class GetCompanyRolesDropDownRequestModel
    : IRequest<GetCompanyRolesDropDownResponseModel> { }

public class GetCompanyRolesDropDownRequestHandler
    : IRequestHandler<GetCompanyRolesDropDownRequestModel, GetCompanyRolesDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyRolesDropDownRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyRolesDropDownResponseModel> Handle(
        GetCompanyRolesDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var roles = await _context
            .CompanyRoles.Where(r => !r.IsDeleted && r.IsActive && r.CompanyId == companyId) // Tenant filter
            .OrderBy(r => r.Name)
            .Select(CompanyRoleSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetCompanyRolesDropDownResponseModel { Data = roles };
    }
}

public class GetCompanyRolesDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
