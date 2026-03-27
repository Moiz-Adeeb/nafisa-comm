using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Products.Commands;

public class DeleteProductRequestModel : IRequest<DeleteProductResponseModel>
{
    public string Id { get; set; }
}

public class DeleteProductRequestModelValidator : AbstractValidator<DeleteProductRequestModel>
{
    public DeleteProductRequestModelValidator()
    {
        RuleFor(x => x.Id).Required();
    }
}

public class DeleteProductRequestHandler
    : IRequestHandler<DeleteProductRequestModel, DeleteProductResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IServiceProvider _serviceProvider;

    public DeleteProductRequestHandler(
        ApplicationDbContext context,
        ISessionService sessionService,
        IBackgroundTaskQueueService taskQueueService,
        IServiceProvider serviceProvider
    )
    {
        _context = context;
        _sessionService = sessionService;
        _taskQueueService = taskQueueService;
        _serviceProvider = serviceProvider;
    }

    public async Task<DeleteProductResponseModel> Handle(
        DeleteProductRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var product = await _context.Product
            .Include(p => p.Images)
            .FirstOrDefaultAsync(u => 
                u.Id == request.Id, 
                cancellationToken
            );
        
        if (product == null) throw new NotFoundException(nameof(product));
        product.IsDeleted = true;
        product.IsActive = false;
        
        var imagePaths = product.Images
            .Select(i => i.Url)
            .Where(url  => !string.IsNullOrEmpty(url))
            .ToList();
        
        _context.Product.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        
        if (imagePaths.Any())
        {
            _taskQueueService.QueueBackgroundWorkItem((Func<CancellationToken, ValueTask>)(async token =>
            {
                // Use a try-catch inside background tasks to prevent process crashes
                try 
                {
                    using var scope = _serviceProvider.CreateScope();
                    var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
            
                    foreach (var path in imagePaths)
                    {
                        await imageService.DeleteImageFromServer(path);
                    }
                }
                catch (Exception ex)
                {
                    // In production, log this to Seq, AppInsights, or a file
                    Console.WriteLine($"Background image cleanup failed: {ex.Message}");
                }
            }));
        }
        
        return new DeleteProductResponseModel();
    }
}

public class DeleteProductResponseModel { }
