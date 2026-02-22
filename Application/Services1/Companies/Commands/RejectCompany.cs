using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.Companies.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.Companies.Commands;

public class RejectCompanyRequestModel : IRequest<RejectCompanyResponseModel>
{
    public string Id { get; set; }
    public string AdminNotes { get; set; }
}

public class RejectCompanyRequestModelValidator : AbstractValidator<RejectCompanyRequestModel>
{
    public RejectCompanyRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.AdminNotes).Required().Max(1000);
    }
}

public class RejectCompanyRequestHandler
    : IRequestHandler<RejectCompanyRequestModel, RejectCompanyResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public RejectCompanyRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<RejectCompanyResponseModel> Handle(
        RejectCompanyRequestModel request,
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
            throw new BadRequestException("Cannot reject an approved company");
        }

        if (company.Status == RequestStatus.Rejected)
        {
            throw new BadRequestException("Company is already rejected");
        }

        // Update company status
        company.Status = RequestStatus.Rejected;
        company.RejectedByUserId = _sessionService.GetUserId();
        company.RejectedDate = DateTimeOffset.UtcNow;
        company.AdminNotes = request.AdminNotes;
        company.IsActive = false;

        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Reject,
                Description =
                    $"Company '{company.CompanyName ?? company.Email}' rejected. Reason: {request.AdminNotes}",
                DescriptionFr =
                    $"Entreprise '{company.CompanyName ?? company.Email}' rejetée. Raison: {request.AdminNotes}",
                EntityId = company.Id,
            }
        );

        // TODO: Send rejection email notification to company

        return new RejectCompanyResponseModel
        {
            Data = new CompanyDto(company),
            Message = "Company rejected successfully.",
        };
    }
}

public class RejectCompanyResponseModel
{
    public CompanyDto Data { get; set; }
    public string Message { get; set; }
}
