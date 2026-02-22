using System.Linq.Expressions;
using Application.Interfaces;
using Application.Services.AuditLogs.Models;
using Application.Shared;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.AuditLogs.Queries;

public class GetAuditLogsRequestModel : GetPagedRequest<GetAuditLogsResponseModel>
{
    public string UserId { get; set; }
    public AuditLogFeatureType? Feature { get; set; } // e.g., "Order", "Product", "Invoice"
    public AuditLogType? Action { get; set; } // "Create", "Update", "Delete"
    public Lang? Lang { get; set; }
    public string EntityId { get; set; }
    public string ParentId { get; set; }
}

public class GetAuditLogsRequestModelValidator : PageRequestValidator<GetAuditLogsRequestModel>
{
    public GetAuditLogsRequestModelValidator() { }
}

public class GetAuditLogsRequestHandler
    : IRequestHandler<GetAuditLogsRequestModel, GetAuditLogsResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;

    public GetAuditLogsRequestHandler(ApplicationDbContext context, ISessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public async Task<GetAuditLogsResponseModel> Handle(
        GetAuditLogsRequestModel request,
        CancellationToken cancellationToken
    )
    {
        Expression<Func<AuditLog, bool>> query = p => true;
        if (request.EntityId != null)
        {
            query = query.AndAlso(p => p.EntityId == request.EntityId);
        }

        if (request.ParentId != null)
        {
            query = query.AndAlso(p => p.ParentId == request.ParentId);
        }
        if (request.Action != null)
        {
            query = query.AndAlso(p => p.Action == request.Action);
        }
        if (request.Feature != null)
        {
            query = query.AndAlso(p => p.Feature == request.Feature);
        }
        if (request.UserId.IsNotNullOrWhiteSpace())
        {
            query = query.AndAlso(p => p.UserId == request.UserId);
        }

        var list = await _context
            .AuditLogs.GetManyReadOnly(query, request)
            .Select(AuditLogSelector.GetSelector(request.Lang))
            .ToListAsync(cancellationToken: cancellationToken);
        var count = await _context.AuditLogs.ActiveCount(
            query,
            cancellationToken: cancellationToken
        );
        return new GetAuditLogsResponseModel() { Data = list, Count = count };
    }
}

public class GetAuditLogsResponseModel
{
    public List<AuditLogDto> Data { get; set; }
    public int Count { get; set; }
}
