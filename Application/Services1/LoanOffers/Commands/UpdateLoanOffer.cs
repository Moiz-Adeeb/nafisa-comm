using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.LoanOffers.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanOffers.Commands;

public class UpdateLoanOfferRequestModel : IRequest<UpdateLoanOfferResponseModel>
{
    public string Id { get; set; }
    public string Title { get; set; }
    public decimal InterestRate { get; set; }
    public decimal LoanMin { get; set; }
    public decimal LoanMax { get; set; }
    public List<int> Durations { get; set; } = new List<int>();
    public string Description { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateLoanOfferRequestModelValidator : AbstractValidator<UpdateLoanOfferRequestModel>
{
    public UpdateLoanOfferRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.Title).Required().Max(100);
        RuleFor(p => p.InterestRate)
            .Required()
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Interest rate must be between 0 and 100");
        RuleFor(p => p.LoanMin)
            .Required()
            .GreaterThan(0)
            .WithMessage("Minimum loan amount must be greater than 0");
        RuleFor(p => p.LoanMax)
            .Required()
            .GreaterThan(0)
            .WithMessage("Maximum loan amount must be greater than 0");
        RuleFor(p => p.LoanMax)
            .GreaterThanOrEqualTo(p => p.LoanMin)
            .WithMessage(
                "Maximum loan amount must be greater than or equal to minimum loan amount"
            );
        RuleFor(p => p.Durations)
            .Must(d => d != null && d.Count > 0)
            .WithMessage("At least one duration must be selected")
            .Must(d => d.All(duration => duration == 6 || duration == 12 || duration == 24))
            .WithMessage("Durations must be 6, 12, or 24 months");
        RuleFor(p => p.Description).Max(1000);
    }
}

public class UpdateLoanOfferRequestHandler
    : IRequestHandler<UpdateLoanOfferRequestModel, UpdateLoanOfferResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public UpdateLoanOfferRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<UpdateLoanOfferResponseModel> Handle(
        UpdateLoanOfferRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can update loan offers."
            );
        }

        var title = request.Title.Trim();

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
            throw new BadRequestException("Cannot update loan offer from a different company");
        }

        // Check if title already exists within the company (excluding current offer)
        var titleExists = await _context.LoanOffers.ActiveAny(
            o => o.CompanyId == companyId && o.Title == title && o.Id != request.Id,
            cancellationToken
        );

        if (titleExists)
        {
            throw new AlreadyExistsException($"Loan offer with title '{title}' already exists");
        }

        // Convert durations list to comma-separated string
        var durationsString = string.Join(",", request.Durations.Distinct().OrderBy(d => d));

        // Update loan offer properties
        loanOffer.Title = title;
        loanOffer.InterestRate = request.InterestRate;
        loanOffer.LoanMin = request.LoanMin;
        loanOffer.LoanMax = request.LoanMax;
        loanOffer.Durations = durationsString;
        loanOffer.Description = request.Description?.Trim();
        loanOffer.IsActive = request.IsActive;

        _context.LoanOffers.Update(loanOffer);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Update,
                Description = $"Loan offer '{loanOffer.Title}' updated",
                DescriptionFr = $"Offre de prêt '{loanOffer.Title}' mise à jour",
                EntityId = loanOffer.Id,
            }
        );

        var dto = new LoanOfferDto(loanOffer);

        return new UpdateLoanOfferResponseModel { Data = dto };
    }
}

public class UpdateLoanOfferResponseModel
{
    public LoanOfferDto Data { get; set; }
}
