using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.CompanyRoles.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyRoles.Queries;

public class GetCompanyRoleByIdRequestModel : IRequest<GetCompanyRoleByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetCompanyRoleByIdRequestModelValidator
    : AbstractValidator<GetCompanyRoleByIdRequestModel>
{
    public GetCompanyRoleByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetCompanyRoleByIdRequestHandler
    : IRequestHandler<GetCompanyRoleByIdRequestModel, GetCompanyRoleByIdResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetCompanyRoleByIdRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetCompanyRoleByIdResponseModel> Handle(
        GetCompanyRoleByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        var role = await _context
            .CompanyRoles.Where(r => r.Id == request.Id && !r.IsDeleted && r.CompanyId == companyId) // Tenant filter
            .Select(CompanyRoleSelector.Selector)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            throw new NotFoundException("Company role not found");
        }

        // Load permissions
        var permissions = await _context
            .CompanyRoleClaims.Where(c => c.CompanyRoleId == role.Id && !c.IsDeleted)
            .Select(c => c.ClaimValue)
            .ToListAsync(cancellationToken);

        role.Permissions = permissions;

        return new GetCompanyRoleByIdResponseModel { Data = role };
    }
}

public class GetCompanyRoleByIdResponseModel
{
    public CompanyRoleDto Data { get; set; }
}
