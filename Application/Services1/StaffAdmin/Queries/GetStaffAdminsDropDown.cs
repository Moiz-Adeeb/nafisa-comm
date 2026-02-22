using Application.Services.StaffAdmin.Models;
using Application.Shared;
using Domain.Constant;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.StaffAdmin.Queries;

public class GetStaffAdminsDropDownRequestModel : IRequest<GetStaffAdminsDropDownResponseModel> { }

public class GetStaffAdminsDropDownRequestHandler
    : IRequestHandler<GetStaffAdminsDropDownRequestModel, GetStaffAdminsDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetStaffAdminsDropDownRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetStaffAdminsDropDownResponseModel> Handle(
        GetStaffAdminsDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var staffAdmins = await _context
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted && u.IsEnabled)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Administrator))
            .OrderBy(u => u.FullName)
            .Select(StaffAdminSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetStaffAdminsDropDownResponseModel { Data = staffAdmins };
    }
}

public class GetStaffAdminsDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
