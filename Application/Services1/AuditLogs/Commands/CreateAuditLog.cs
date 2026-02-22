using Application.Extensions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Persistence.Context;

namespace Application.Services.AuditLogs.Commands;

public class CreateAuditLogRequestModel : IRequest<CreateAuditLogResponseModel>
{
    public string UserId { get; set; }
    public AuditLogFeatureType Feature { get; set; } // e.g., "Order", "Product", "Invoice"
    public AuditLogType Action { get; set; } // "Create", "Update", "Delete"
    public string Description { get; set; } // e.g., "Order #1023 created with 3 items"
    public string DescriptionFr { get; set; } // e.g., "Order #1023 created with 3 items"
    public string EntityId { get; set; } // Optional: ID of the object affected
    public string ParentId { get; set; }
}

public class CreateAuditLogRequestModelValidator : AbstractValidator<CreateAuditLogRequestModel>
{
    public CreateAuditLogRequestModelValidator()
    {
        RuleFor(x => x.UserId).Required();
        RuleFor(x => x.Description).Required().Max(255);
        RuleFor(x => x.Feature).IsInEnum();
        RuleFor(x => x.Action).IsInEnum();
    }
}

public class CreateAuditLogRequestHandler
    : IRequestHandler<CreateAuditLogRequestModel, CreateAuditLogResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public CreateAuditLogRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService
    )
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<CreateAuditLogResponseModel> Handle(
        CreateAuditLogRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var log = new AuditLog()
        {
            Action = request.Action,
            EntityId = request.EntityId,
            Description = request.Description,
            DescriptionFr = request.DescriptionFr,
            Feature = request.Feature,
            UserId = request.UserId,
            ParentId = request.ParentId,
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
        return new CreateAuditLogResponseModel();
    }
}

public class CreateAuditLogResponseModel { }
