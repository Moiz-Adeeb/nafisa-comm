using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.LoanOffers.Models;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanOffers.Commands;

public class CreateLoanOfferRequestModel : IRequest<CreateLoanOfferResponseModel>
{
    public string Title { get; set; }
    public decimal InterestRate { get; set; }
    public decimal LoanMin { get; set; }
    public decimal LoanMax { get; set; }
    public List<int> Durations { get; set; } = new List<int>(); // e.g., [6, 12, 24]
    public string Description { get; set; }
}

public class CreateLoanOfferRequestModelValidator : AbstractValidator<CreateLoanOfferRequestModel>
{
    public CreateLoanOfferRequestModelValidator()
    {
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

public class CreateLoanOfferRequestHandler
    : IRequestHandler<CreateLoanOfferRequestModel, CreateLoanOfferResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public CreateLoanOfferRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<CreateLoanOfferResponseModel> Handle(
        CreateLoanOfferRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company users can create loan offers."
            );
        }

        var title = request.Title.Trim();

        // Check if loan offer with same title already exists within the company
        var titleExists = await _context.LoanOffers.ActiveAny(
            o => o.CompanyId == companyId && o.Title == title,
            cancellationToken
        );

        if (titleExists)
        {
            throw new AlreadyExistsException($"Loan offer with title '{title}' already exists");
        }

        // Convert durations list to comma-separated string
        var durationsString = string.Join(",", request.Durations.Distinct().OrderBy(d => d));

        // Create loan offer
        var loanOffer = new LoanOffer
        {
            CompanyId = companyId,
            Title = title,
            InterestRate = request.InterestRate,
            LoanMin = request.LoanMin,
            LoanMax = request.LoanMax,
            Durations = durationsString,
            Description = request.Description?.Trim(),
            IsActive = true,
        };

        await _context.LoanOffers.AddAsync(loanOffer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description =
                    $"Loan offer '{loanOffer.Title}' created with rate {loanOffer.InterestRate}%",
                DescriptionFr =
                    $"Offre de prêt '{loanOffer.Title}' créée avec un taux de {loanOffer.InterestRate}%",
                EntityId = loanOffer.Id,
            }
        );

        var dto = new LoanOfferDto(loanOffer);

        return new CreateLoanOfferResponseModel { Data = dto };
    }
}

public class CreateLoanOfferResponseModel
{
    public LoanOfferDto Data { get; set; }
}
