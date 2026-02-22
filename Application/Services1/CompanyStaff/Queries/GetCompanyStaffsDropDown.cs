using Application.Interfaces;
using Application.Services.CompanyStaff.Models;
using Application.Shared;
using Domain.Constant;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyStaff.Queries;

public class GetCompanyStaffsDropDownRequestModel
    : IRequest<GetCompanyStaffsDropDownResponseModel> { }

public class GetCompanyStaffsDropDownRequestHandler
    : IRequestHandler<GetCompanyStaffsDropDownRequestModel, GetCompanyStaffsDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyStaffsDropDownRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyStaffsDropDownResponseModel> Handle(
        GetCompanyStaffsDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var companyStaffs = await _context
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted && u.IsEnabled)
            .Where(u =>
                u.UserRoles.Any(ur =>
                    ur.Role.Name == RoleNames.CompanyAdmin || ur.Role.Name == RoleNames.Employee
                )
            )
            // .Where(u => u.CompanyId == companyId) // Tenant filter if entity supports CompanyId
            .OrderBy(u => u.FullName)
            .Select(CompanyStaffSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetCompanyStaffsDropDownResponseModel { Data = companyStaffs };
    }
}

public class GetCompanyStaffsDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
