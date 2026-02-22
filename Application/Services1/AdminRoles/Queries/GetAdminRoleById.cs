using Application.Exceptions;
using Application.Extensions;
using Application.Services.AdminRoles.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.AdminRoles.Queries;

public class GetAdminRoleByIdRequestModel : IRequest<GetAdminRoleByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetAdminRoleByIdRequestModelValidator : AbstractValidator<GetAdminRoleByIdRequestModel>
{
    public GetAdminRoleByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetAdminRoleByIdRequestHandler
    : IRequestHandler<GetAdminRoleByIdRequestModel, GetAdminRoleByIdResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetAdminRoleByIdRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdminRoleByIdResponseModel> Handle(
        GetAdminRoleByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var role = await _context
            .AdminRoles.Include(r => r.Users)
            .Where(r => r.Id == request.Id && !r.IsDeleted)
            .Select(AdminRoleSelector.Selector)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            throw new NotFoundException("Admin role not found");
        }

        // Load permissions
        var permissions = await _context
            .AdminRoleClaims.Where(c => c.AdminRoleId == role.Id && !c.IsDeleted)
            .Select(c => c.ClaimValue)
            .ToListAsync(cancellationToken);

        role.Permissions = permissions;

        return new GetAdminRoleByIdResponseModel { Data = role };
    }
}

public class GetAdminRoleByIdResponseModel
{
    public AdminRoleDto Data { get; set; }
}
