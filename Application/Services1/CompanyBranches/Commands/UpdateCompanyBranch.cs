using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.CompanyBranches.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.CompanyBranches.Commands;

public class UpdateCompanyBranchRequestModel : IRequest<UpdateCompanyBranchResponseModel>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

public class UpdateCompanyBranchRequestModelValidator
    : AbstractValidator<UpdateCompanyBranchRequestModel>
{
    public UpdateCompanyBranchRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.Name).Required().Max(100);
        RuleFor(p => p.Code).Required().Max(50);
        RuleFor(p => p.PhoneNumber).Max(20);
        RuleFor(p => p.Address).Max(500);
    }
}

public class UpdateCompanyBranchRequestHandler
    : IRequestHandler<UpdateCompanyBranchRequestModel, UpdateCompanyBranchResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public UpdateCompanyBranchRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<UpdateCompanyBranchResponseModel> Handle(
        UpdateCompanyBranchRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can update branches."
            );
        }

        var branchName = request.Name.Trim();
        var branchCode = request.Code.Trim().ToUpper();

        // Get existing branch
        var branch = await _context.CompanyBranches.FirstOrDefaultAsync(
            b => b.Id == request.Id && !b.IsDeleted,
            cancellationToken
        );

        if (branch == null)
        {
            throw new NotFoundException("Branch not found");
        }

        // Validate tenant ownership
        if (branch.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot update branch from a different company");
        }

        // Check if name already exists within the company (excluding current branch)
        var nameExists = await _context.CompanyBranches.AnyAsync(
            b =>
                b.CompanyId == companyId
                && b.Name == branchName
                && b.Id != request.Id
                && !b.IsDeleted,
            cancellationToken
        );

        if (nameExists)
        {
            throw new AlreadyExistsException($"Branch with name '{branchName}' already exists");
        }

        // Check if code already exists within the company (excluding current branch)
        var codeExists = await _context.CompanyBranches.AnyAsync(
            b =>
                b.CompanyId == companyId
                && b.Code == branchCode
                && b.Id != request.Id
                && !b.IsDeleted,
            cancellationToken
        );

        if (codeExists)
        {
            throw new AlreadyExistsException($"Branch with code '{branchCode}' already exists");
        }

        // Update branch properties
        branch.Name = branchName;
        branch.Code = branchCode;
        branch.PhoneNumber = request.PhoneNumber?.Trim();
        branch.Address = request.Address?.Trim();

        _context.CompanyBranches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description = $"Branch '{branch.Name}' updated",
                DescriptionFr = $"Succursale '{branch.Name}' mise à jour",
                EntityId = branch.Id,
            }
        );

        var dto = new CompanyBranchDto(branch);

        return new UpdateCompanyBranchResponseModel { Data = dto };
    }
}

public class UpdateCompanyBranchResponseModel
{
    public CompanyBranchDto Data { get; set; }
}
