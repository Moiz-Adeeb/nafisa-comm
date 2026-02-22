using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.LoanRequests.Models;
using Domain.Entities;
using Domain.Enums;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.LoanRequests.Commands;

public class CreateLoanRequestRequestModel : IRequest<CreateLoanRequestResponseModel>
{
    public string LoanOfferId { get; set; } // Optional - for predefined offers
    public string Title { get; set; }
    public decimal Amount { get; set; }
    public int Duration { get; set; } // 6, 12, or 24 months
    public decimal InterestRate { get; set; }
    public string Purpose { get; set; }
    public string Document { get; set; } // Base64 or file path
}

public class CreateLoanRequestRequestModelValidator
    : AbstractValidator<CreateLoanRequestRequestModel>
{
    public CreateLoanRequestRequestModelValidator()
    {
        RuleFor(p => p.Title).Required().Max(200);
        RuleFor(p => p.Amount)
            .Required()
            .GreaterThan(0)
            .WithMessage("Loan amount must be greater than 0");
        RuleFor(p => p.Duration)
            .Required()
            .Must(d => d == 6 || d == 12 || d == 24)
            .WithMessage("Duration must be 6, 12, or 24 months");
        RuleFor(p => p.InterestRate)
            .Required()
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Interest rate must be between 0 and 100");
        RuleFor(p => p.Purpose).Required().Max(1000);
    }
}

public class CreateLoanRequestRequestHandler
    : IRequestHandler<CreateLoanRequestRequestModel, CreateLoanRequestResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public CreateLoanRequestRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IImageService imageService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _imageService = imageService;
    }

    public async Task<CreateLoanRequestResponseModel> Handle(
        CreateLoanRequestRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var userId = _sessionService.GetUserId();
        var companyId = _sessionService.GetCompanyId();

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new BadRequestException(
                "Company ID not found. Only company employees can create loan requests."
            );
        }

        // Validate loan offer if provided
        if (!string.IsNullOrWhiteSpace(request.LoanOfferId))
        {
            var loanOffer = await _context.LoanOffers.FirstOrDefaultAsync(
                o =>
                    o.Id == request.LoanOfferId
                    && !o.IsDeleted
                    && o.IsActive
                    && o.CompanyId == companyId,
                cancellationToken
            );

            if (loanOffer == null)
            {
                throw new NotFoundException("Loan offer not found or inactive");
            }

            // Validate amount is within offer limits
            if (request.Amount < loanOffer.LoanMin || request.Amount > loanOffer.LoanMax)
            {
                throw new BadRequestException(
                    $"Loan amount must be between {loanOffer.LoanMin} and {loanOffer.LoanMax}"
                );
            }

            // Validate duration is available in offer
            var availableDurations = loanOffer
                .Durations?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => int.TryParse(d.Trim(), out var duration) ? duration : 0)
                .Where(d => d > 0)
                .ToList();

            if (availableDurations == null || !availableDurations.Contains(request.Duration))
            {
                throw new BadRequestException(
                    $"Duration {request.Duration} months is not available for this loan offer"
                );
            }
        }

        // Calculate loan amounts
        var totalPayback = request.Amount + (request.Amount * request.InterestRate / 100);
        var monthlyPayment = Math.Ceiling(totalPayback / request.Duration);

        // Save document if provided
        string documentPath = null;
        if (!string.IsNullOrWhiteSpace(request.Document))
        {
            documentPath = await _imageService.SaveImageToServer(
                request.Document,
                ".pdf",
                "loan-requests"
            );
        }

        // Create loan request
        var loanRequest = new LoanRequest
        {
            CompanyId = companyId,
            CompanyStaffId = userId,
            LoanOfferId = request.LoanOfferId,
            Title = request.Title.Trim(),
            Amount = request.Amount,
            Duration = request.Duration,
            InterestRate = request.InterestRate,
            TotalPayback = totalPayback,
            MonthlyPayment = monthlyPayment,
            Purpose = request.Purpose.Trim(),
            DocumentPath = documentPath,
            Status = LoanRequestStatus.Pending,
        };

        await _context.LoanRequests.AddAsync(loanRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = userId,
                Feature = AuditLogFeatureType.Company,
                Action = AuditLogType.Create,
                Description =
                    $"Loan request '{loanRequest.Title}' created for {loanRequest.Amount:C}",
                DescriptionFr =
                    $"Demande de prêt '{loanRequest.Title}' créée pour {loanRequest.Amount:C}",
                EntityId = loanRequest.Id,
            }
        );

        var dto = new LoanRequestDto(loanRequest);

        return new CreateLoanRequestResponseModel { Data = dto };
    }
}

public class CreateLoanRequestResponseModel
{
    public LoanRequestDto Data { get; set; }
}
