using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Commands;

public class DeleteCategoryRequestModel : IRequest<DeleteCategoryResponseModel>
{
    public string Id { get; set; }
}

public class DeleteCategoryRequestModelValidator : AbstractValidator<DeleteCategoryRequestModel>
{
    public DeleteCategoryRequestModelValidator()
    {
        RuleFor(x => x.Id).Required();
    }
}

public class DeleteCategoryRequestHandler
    : IRequestHandler<DeleteCategoryRequestModel, DeleteCategoryResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;

    public DeleteCategoryRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
    }

    public async Task<DeleteCategoryResponseModel> Handle(
        DeleteCategoryRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var category = await _context.Category.GetByAsync(
            u => u.Id == request.Id,
            cancellationToken: cancellationToken
        );
        
        if (category == null) throw new NotFoundException(nameof(category));
        category.IsDeleted = true;
        
        _context.Category.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new DeleteCategoryResponseModel();
    }
}

public class DeleteCategoryResponseModel { }