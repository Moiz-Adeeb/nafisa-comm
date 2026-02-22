using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Domain.Constant;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.StaffAdmin.Commands;

public class DeleteStaffAdminRequestModel : IRequest<DeleteStaffAdminResponseModel>
{
    public string Id { get; set; }
}

public class DeleteStaffAdminRequestModelValidator : AbstractValidator<DeleteStaffAdminRequestModel>
{
    public DeleteStaffAdminRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
    }
}

public class DeleteStaffAdminRequestHandler
    : IRequestHandler<DeleteStaffAdminRequestModel, DeleteStaffAdminResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteStaffAdminRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteStaffAdminResponseModel> Handle(
        DeleteStaffAdminRequestModel request,
        CancellationToken cancellationToken
    )
    {
        // Get existing user
        var user = await _context
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Admin staff not found");
        }

        // Verify user is an admin staff (has Administrator role)
        var isAdminStaff = user.UserRoles.Any(ur => ur.Role.Name == RoleNames.Administrator);
        if (!isAdminStaff)
        {
            throw new BadRequestException("User is not an admin staff member");
        }

        // Prevent self-deletion
        var currentUserId = _sessionService.GetUserId();
        if (user.Id == currentUserId)
        {
            throw new BadRequestException("You cannot delete your own account");
        }

        // Soft delete
        user.IsDeleted = true;
        user.IsEnabled = false;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = currentUserId,
                Feature = AuditLogFeatureType.User,
                Action = AuditLogType.Delete,
                Description = $"Admin staff '{user.FullName}' deleted",
                DescriptionFr = $"Personnel administrateur '{user.FullName}' supprimé",
                EntityId = user.Id,
            }
        );

        return new DeleteStaffAdminResponseModel { Success = true };
    }
}

public class DeleteStaffAdminResponseModel
{
    public bool Success { get; set; }
}
