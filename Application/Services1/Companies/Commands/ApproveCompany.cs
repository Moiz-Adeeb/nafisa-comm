using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Companies.Models;
using Domain.Constant;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Companies.Commands;

public class ApproveCompanyRequestModel : IRequest<ApproveCompanyResponseModel>
{
    public string Id { get; set; }
    public string AdminNotes { get; set; }
}

public class ApproveCompanyRequestModelValidator : AbstractValidator<ApproveCompanyRequestModel>
{
    public ApproveCompanyRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.AdminNotes).Max(1000);
    }
}

public class ApproveCompanyRequestHandler
    : IRequestHandler<ApproveCompanyRequestModel, ApproveCompanyResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly UserManager<User> _userManager;

    public ApproveCompanyRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        UserManager<User> userManager
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _userManager = userManager;
    }

    public async Task<ApproveCompanyResponseModel> Handle(
        ApproveCompanyRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var company = await _context.Companies.FirstOrDefaultAsync(
            c => c.Id == request.Id && !c.IsDeleted,
            cancellationToken
        );

        if (company == null)
        {
            throw new NotFoundException("Company not found");
        }

        if (company.Status == RequestStatus.Approved)
        {
            throw new BadRequestException("Company is already approved");
        }

        if (company.Status == RequestStatus.Rejected)
        {
            throw new BadRequestException(
                "Cannot approve a rejected company. Please register again."
            );
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(company.Email);
        if (existingUser != null)
        {
            throw new AlreadyExistsException($"User with email '{company.Email}' already exists");
        }

        // Create CompanyAdmin user account
        var user = new User
        {
            Email = company.Email,
            FullName = $"{company.FirstName} {company.LastName}",
            NormalizedEmail = company.Email.ToUpper(),
            UserName = company.Email,
            NormalizedUserName = company.Email.ToUpper(),
            PhoneNumber = company.PhoneNumber,
            IsEnabled = true,
            EmailConfirmed = true,
        };

        // Set a temporary password (company should reset it)
        var result = await _userManager.CreateAsync(user, "TempPassword@123");
        if (!result.Succeeded)
        {
            throw new BadRequestException(
                $"Failed to create user account: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        // Assign CompanyAdmin role
        await _userManager.AddToRoleAsync(user, RoleNames.CompanyAdmin);

        // Update company status
        company.Status = RequestStatus.Approved;
        company.ApprovedByUserId = _sessionService.GetUserId();
        company.ApprovedDate = DateTimeOffset.UtcNow;
        company.AdminNotes = request.AdminNotes;
        company.CompanyAdminUserId = user.Id;
        company.IsActive = true; // Activate company after approval

        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Approve,
                Description =
                    $"Company '{company.CompanyName ?? company.Email}' approved and user account created",
                DescriptionFr =
                    $"Entreprise '{company.CompanyName ?? company.Email}' approuvée et compte utilisateur créé",
                EntityId = company.Id,
            }
        );

        // TODO: Send approval email notification to company

        return new ApproveCompanyResponseModel
        {
            Data = new CompanyDto(company),
            Message = "Company approved successfully. User account created.",
        };
    }
}

public class ApproveCompanyResponseModel
{
    public CompanyDto Data { get; set; }
    public string Message { get; set; }
}
