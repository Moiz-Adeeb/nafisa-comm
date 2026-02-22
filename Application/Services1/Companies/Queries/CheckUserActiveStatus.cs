using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Companies.Queries;

public class CheckUserActiveStatusRequestModel : IRequest<CheckUserActiveStatusResponseModel> { }

public class CheckUserActiveStatusRequestHandler
    : IRequestHandler<CheckUserActiveStatusRequestModel, CheckUserActiveStatusResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CheckUserActiveStatusRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CheckUserActiveStatusResponseModel> Handle(
        CheckUserActiveStatusRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();

        // Check if user is active
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Id == userId && !u.IsDeleted,
            cancellationToken
        );

        if (user == null)
        {
            return new CheckUserActiveStatusResponseModel
            {
                IsUserActive = false,
                IsCompanyActive = false,
                Message = "User not found",
            };
        }

        if (!user.IsEnabled)
        {
            return new CheckUserActiveStatusResponseModel
            {
                IsUserActive = false,
                IsCompanyActive = false,
                Message = "User account is disabled",
            };
        }

        // Check if user belongs to a company and if that company is active
        var company = await _context.Companies.FirstOrDefaultAsync(
            c => c.CompanyAdminUserId == userId && !c.IsDeleted,
            cancellationToken
        );

        if (company == null)
        {
            // User doesn't belong to any company (might be admin or employee)
            return new CheckUserActiveStatusResponseModel
            {
                IsUserActive = true,
                IsCompanyActive = true, // N/A for non-company users
                Message = "User is active",
            };
        }

        if (!company.IsActive)
        {
            return new CheckUserActiveStatusResponseModel
            {
                IsUserActive = true,
                IsCompanyActive = false,
                CompanyId = company.Id,
                CompanyName = company.CompanyName ?? $"{company.FirstName} {company.LastName}",
                Message = "Company is inactive",
            };
        }

        return new CheckUserActiveStatusResponseModel
        {
            IsUserActive = true,
            IsCompanyActive = true,
            CompanyId = company.Id,
            CompanyName = company.CompanyName ?? $"{company.FirstName} {company.LastName}",
            Message = "User and company are active",
        };
    }
}

public class CheckUserActiveStatusResponseModel
{
    public bool IsUserActive { get; set; }
    public bool IsCompanyActive { get; set; }
    public string CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string Message { get; set; }
}
