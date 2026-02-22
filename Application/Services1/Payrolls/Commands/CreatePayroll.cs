using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Payrolls.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Payrolls.Commands;

public class CreatePayrollRequestModel : IRequest<CreatePayrollResponseModel>
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<string> CompanyStaffIds { get; set; }
    public List<string> BranchIds { get; set; } // Optional filter
}

public class CreatePayrollRequestModelValidator : AbstractValidator<CreatePayrollRequestModel>
{
    public CreatePayrollRequestModelValidator()
    {
        RuleFor(p => p.Month).Required().InclusiveBetween(1, 12);
        RuleFor(p => p.Year).Required().GreaterThan(2000).LessThanOrEqualTo(2100);
        RuleFor(p => p.CompanyStaffIds).Required().Must(x => x != null && x.Count > 0)
            .WithMessage("At least one company staff must be selected");
    }
}

public class CreatePayrollRequestHandler
    : IRequestHandler<CreatePayrollRequestModel, CreatePayrollResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public CreatePayrollRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<CreatePayrollResponseModel> Handle(
        CreatePayrollRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can create payroll."
            );
        }

        // Verify all staff members exist and belong to the company
        var companyStaffs = await _context
            .CompanyStaffs.Include(cs => cs.User)
            .Include(cs => cs.Branch)
            .Where(cs =>
                request.CompanyStaffIds.Contains(cs.UserId) && cs.CompanyId == companyId
            )
            .ToListAsync(cancellationToken);

        if (companyStaffs.Count != request.CompanyStaffIds.Count)
        {
            throw new NotFoundException("One or more company staff not found");
        }

        // Check if payroll already exists for any of these staff members for the given month/year
        var existingPayrolls = await _context
            .PayRolls.Where(p =>
                p.CompanyId == companyId
                && p.Month == request.Month
                && p.Year == request.Year
                && request.CompanyStaffIds.Contains(p.CompanyStaffId)
            )
            .ToListAsync(cancellationToken);

        if (existingPayrolls.Any())
        {
            var staffNames = companyStaffs
                .Where(cs => existingPayrolls.Select(p => p.CompanyStaffId).Contains(cs.UserId))
                .Select(cs => cs.User.FullName)
                .ToList();
            throw new AlreadyExistsException(
                $"Payroll already exists for {string.Join(", ", staffNames)} for {request.Month}/{request.Year}"
            );
        }

        // Get all loan payment schedules for the given month/year for these staff members
        var loanPaymentSchedules = await _context
            .Set<LoanPaymentSchedule>()
            .Where(lps =>
                lps.CompanyId == companyId
                && lps.Month == request.Month
                && lps.Year == request.Year
                && request.CompanyStaffIds.Contains(lps.CompanyStaffId)
                && !lps.IsPayed
            )
            .ToListAsync(cancellationToken);

        var payrolls = new List<PayRoll>();

        foreach (var staff in companyStaffs)
        {
            // Calculate total loan deduction for this staff member
            var loanDeduction = loanPaymentSchedules
                .Where(lps => lps.CompanyStaffId == staff.UserId)
                .Sum(lps => lps.Amount);

            var payroll = new PayRoll
            {
                CompanyId = companyId,
                CompanyStaffId = staff.UserId,
                Month = request.Month,
                Year = request.Year,
                Amount = staff.Salary,
                LoanDeduction = loanDeduction,
                NetAmount = staff.Salary - loanDeduction,
            };

            payrolls.Add(payroll);
        }

        await _context.PayRolls.AddRangeAsync(payrolls, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description =
                    $"Payroll created for {payrolls.Count} staff members for {request.Month}/{request.Year}",
                DescriptionFr =
                    $"Paie créée pour {payrolls.Count} employés pour {request.Month}/{request.Year}",
            }
        );

        var dtos = payrolls
            .Select(p =>
            {
                var staff = companyStaffs.First(cs => cs.UserId == p.CompanyStaffId);
                return new PayrollDto
                {
                    Id = p.Id,
                    CompanyId = p.CompanyId,
                    Month = p.Month,
                    Year = p.Year,
                    CompanyStaffId = p.CompanyStaffId,
                    StaffName = staff.User.FullName,
                    StaffEmail = staff.User.Email,
                    BranchId = staff.BranchId,
                    BranchName = staff.Branch?.Name,
                    Amount = p.Amount,
                    LoanDeduction = p.LoanDeduction,
                    NetAmount = p.NetAmount,
                    CreatedDate = p.CreatedDate,
                };
            })
            .ToList();

        return new CreatePayrollResponseModel { Data = dtos };
    }
}

public class CreatePayrollResponseModel
{
    public List<PayrollDto> Data { get; set; }
}