using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Categories.Models;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Commands;

public class UpdateCategoryRequestModel : IRequest<UpdateCategoryResponseModel>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateCategoryRequestModelValidator : AbstractValidator<UpdateCategoryRequestModel>
{
    public UpdateCategoryRequestModelValidator()
    {
        RuleFor(p => p.Name).Required().Max(50);
        RuleFor(p => p.Description).Max(500);
    }
}

public class UpdateCategoryRequestHandler
    : IRequestHandler<UpdateCategoryRequestModel, UpdateCategoryResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public UpdateCategoryRequestHandler(
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

    public async Task<UpdateCategoryResponseModel> Handle(
        UpdateCategoryRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var category = await _context.Category.GetByAsync(
            u => u.Id == request.Id,
            cancellationToken: cancellationToken
        );
        var name = request.Name.ToLower();
        if (name != category.Name)
        {
            var isNameTaken = await _context.Category.ActiveAny(
                u => u.Name == name, 
                cancellationToken
            );
            
            if (isNameTaken) throw new AlreadyExistsException(nameof(name));
            category.Name = name;
        }
        
        if (request.Description != category.Description && request.Description != null)
        {
            category.Description = request.Description;
        }

        _context.Category.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdateCategoryResponseModel() { Data = new CategoryDto(category) };
    }
}

public class UpdateCategoryResponseModel
{
    public CategoryDto Data { get; set; }
}