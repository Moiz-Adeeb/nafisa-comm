using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.AuditLogs.Commands;
using Application.Services.StaffAdmin.Models;
using Domain.Constant;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Application.Services.StaffAdmin.Commands;

public class UpdateStaffAdminRequestModel : IRequest<UpdateStaffAdminResponseModel>
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Image { get; set; }
    public string AdminRoleId { get; set; } // Optional custom admin role
    public bool IsEnabled { get; set; }
}

public class UpdateStaffAdminRequestModelValidator : AbstractValidator<UpdateStaffAdminRequestModel>
{
    public UpdateStaffAdminRequestModelValidator()
    {
        RuleFor(p => p.Id).Required();
        RuleFor(p => p.FullName).Required().Max(100);
        RuleFor(p => p.Email).Required().EmailAddress().Max(100);
        RuleFor(p => p.PhoneNumber).Max(20);
    }
}

public class UpdateStaffAdminRequestHandler
    : IRequestHandler<UpdateStaffAdminRequestModel, UpdateStaffAdminResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public UpdateStaffAdminRequestHandler(
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

    public async Task<UpdateStaffAdminResponseModel> Handle(
        UpdateStaffAdminRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.ToLower().Trim();

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

        // Check if email already exists (excluding current user)
        var emailExists = await _context.Users.AnyAsync(
            u => u.Email == email && u.Id != request.Id && !u.IsDeleted,
            cancellationToken
        );

        if (emailExists)
        {
            throw new AlreadyExistsException($"User with email '{email}' already exists");
        }

        // Validate AdminRoleId if provided
        if (!string.IsNullOrWhiteSpace(request.AdminRoleId))
        {
            var adminRoleExists = await _context.AdminRoles.AnyAsync(
                r => r.Id == request.AdminRoleId && !r.IsDeleted && r.IsActive,
                cancellationToken
            );

            if (!adminRoleExists)
            {
                throw new NotFoundException("Admin role not found or inactive");
            }
        }

        // Update user properties
        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.NormalizedEmail = email.ToUpper();
        user.UserName = email;
        user.NormalizedUserName = email.ToUpper();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.AdminRoleId = request.AdminRoleId;
        user.IsEnabled = request.IsEnabled;

        // Update image if provided
        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            user.Image = await _imageService.SaveImageToServer(
                request.Image,
                ".png",
                "staff-admin"
            );
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _taskQueueService.QueueBackgroundWorkItem(
            new CreateAuditLogRequestModel
            {
                UserId = _sessionService.GetUserId(),
                Feature = AuditLogFeatureType.User,
                Action = AuditLogType.Update,
                Description = $"Admin staff '{user.FullName}' updated",
                DescriptionFr = $"Personnel administrateur '{user.FullName}' mis à jour",
                EntityId = user.Id,
            }
        );

        var dto = new StaffAdminDto(user) { RoleName = RoleNames.Administrator };

        return new UpdateStaffAdminResponseModel { Data = dto };
    }
}

public class UpdateStaffAdminResponseModel
{
    public StaffAdminDto Data { get; set; }
}
