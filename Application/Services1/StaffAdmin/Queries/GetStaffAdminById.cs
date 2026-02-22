using Application.Exceptions;
using Application.Extensions;
using Application.Services.StaffAdmin.Models;
using Domain.Constant;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.StaffAdmin.Queries;

public class GetStaffAdminByIdRequestModel : IRequest<GetStaffAdminByIdResponseModel>
{
    public string Id { get; set; }
}

public class GetStaffAdminByIdRequestModelValidator
    : AbstractValidator<GetStaffAdminByIdRequestModel>
{
    public GetStaffAdminByIdRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class GetStaffAdminByIdRequestHandler
    : IRequestHandler<GetStaffAdminByIdRequestModel, GetStaffAdminByIdResponseModel>
{
    private readonly ApplicationDbContext _context;

    public GetStaffAdminByIdRequestHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetStaffAdminByIdResponseModel> Handle(
        GetStaffAdminByIdRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var staffAdmin = await _context
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.AdminRole)
            .Where(u => u.Id == request.Id && !u.IsDeleted)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Administrator))
            .Select(StaffAdminSelector.Selector)
            .FirstOrDefaultAsync(cancellationToken);

        if (staffAdmin == null)
        {
            throw new NotFoundException("Admin staff not found");
        }

        return new GetStaffAdminByIdResponseModel { Data = staffAdmin };
    }
}

public class GetStaffAdminByIdResponseModel
{
    public StaffAdminDto Data { get; set; }
}
