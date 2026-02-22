using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanOffers.Commands;

public class DeleteLoanOfferRequestModel : IRequest<DeleteLoanOfferResponseModel>
{
    public string Id { get; set; }
}

public class DeleteLoanOfferRequestModelValidator : AbstractValidator<DeleteLoanOfferRequestModel>
{
    public DeleteLoanOfferRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteLoanOfferRequestHandler
    : IRequestHandler<DeleteLoanOfferRequestModel, DeleteLoanOfferResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteLoanOfferRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteLoanOfferResponseModel> Handle(
        DeleteLoanOfferRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can delete loan offers."
            );
        }

        // Get existing loan offer
        var loanOffer = await _context.LoanOffers.FirstOrDefaultAsync(
            o => o.Id == request.Id && !o.IsDeleted,
            cancellationToken
        );

        if (loanOffer == null)
        {
            throw new NotFoundException("Loan offer not found");
        }

        // Validate tenant ownership
        if (loanOffer.CompanyId != companyId)
        {
            throw new BadRequestException("Cannot delete loan offer from a different company");
        }

        // Check if loan offer has active loan requests
        var hasActiveRequests = await _context.LoanRequests.ActiveAny(
            r => r.LoanOfferId == request.Id,
            cancellationToken
        );

        if (hasActiveRequests)
        {
            throw new BadRequestException(
                "Cannot delete loan offer that has active loan requests. Please deactivate the offer instead."
            );
        }

        // Soft delete
        loanOffer.IsDeleted = true;
        _context.LoanOffers.Update(loanOffer);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Delete,
                Description = $"Loan offer '{loanOffer.Title}' deleted",
                DescriptionFr = $"Offre de prêt '{loanOffer.Title}' supprimée",
                EntityId = loanOffer.Id,
            }
        );

        return new DeleteLoanOfferResponseModel { Success = true };
    }
}

public class DeleteLoanOfferResponseModel
{
    public bool Success { get; set; }
}
