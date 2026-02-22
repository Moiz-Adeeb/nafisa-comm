using Application.Services.AdminRoles.Models;
using Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.AdminRoles.Queries;

public class GetAdminRolesDropDownRequestModel : IRequest<GetAdminRolesDropDownResponseModel> { }

public class GetAdminRolesDropDownRequestHandler
    : IRequestHandler<GetAdminRolesDropDownRequestModel, GetAdminRolesDropDownResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetAdminRolesDropDownRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdminRolesDropDownResponseModel> Handle(
        GetAdminRolesDropDownRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var roles = await _context
            .AdminRoles.Where(r => !r.IsDeleted && r.IsActive)
            .OrderBy(r => r.Name)
            .Select(AdminRoleSelector.SelectorDropDown)
            .ToListAsync(cancellationToken);

        return new GetAdminRolesDropDownResponseModel { Data = roles };
    }
}

public class GetAdminRolesDropDownResponseModel
{
    public List<DropDownDto<string>> Data { get; set; }
}
