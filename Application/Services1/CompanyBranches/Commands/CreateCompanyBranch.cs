using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.CompanyBranches.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.CompanyBranches.Commands;

public class CreateCompanyBranchRequestModel : IRequest<CreateCompanyBranchResponseModel>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

public class CreateCompanyBranchRequestModelValidator
    : AbstractValidator<CreateCompanyBranchRequestModel>
{
    public CreateCompanyBranchRequestModelValidator()
    {
        RuleFor(p => p.Name).Required().Max(100);
        RuleFor(p => p.Code).Required().Max(50);
        RuleFor(p => p.PhoneNumber).Max(20);
        RuleFor(p => p.Address).Max(500);
    }
}

public class CreateCompanyBranchRequestHandler
    : IRequestHandler<CreateCompanyBranchRequestModel, CreateCompanyBranchResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public CreateCompanyBranchRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<CreateCompanyBranchResponseModel> Handle(
        CreateCompanyBranchRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can create branches."
            );
        }

        var branchName = request.Name.Trim();
        var branchCode = request.Code.Trim().ToUpper();

        // Check if branch name already exists within the company
        var nameExists = await _context.CompanyBranches.AnyAsync(
            b => b.CompanyId == companyId && b.Name == branchName && !b.IsDeleted,
            cancellationToken
        );

        if (nameExists)
        {
            throw new AlreadyExistsException($"Branch with name '{branchName}' already exists");
        }

        // Check if branch code already exists within the company
        var codeExists = await _context.CompanyBranches.ActiveAny(
            b => b.CompanyId == companyId && b.Code == branchCode,
            cancellationToken
        );

        if (codeExists)
        {
            throw new AlreadyExistsException($"Branch with code '{branchCode}' already exists");
        }

        // Create branch
        var branch = new CompanyBranch
        {
            CompanyId = companyId,
            Name = branchName,
            Code = branchCode,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
        };

        await _context.CompanyBranches.AddAsync(branch, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description = $"Branch '{branch.Name}' created with code '{branch.Code}'",
                DescriptionFr = $"Succursale '{branch.Name}' créée avec le code '{branch.Code}'",
                EntityId = branch.Id,
            }
        );

        var dto = new CompanyBranchDto(branch);

        return new CreateCompanyBranchResponseModel { Data = dto };
    }
}

public class CreateCompanyBranchResponseModel
{
    public CompanyBranchDto Data { get; set; }
}
