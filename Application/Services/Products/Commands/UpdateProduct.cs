using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Products.Models;
using Common.Extensions;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Products.Commands;

public class UpdateProductRequestModel : IRequest<UpdateProductResponseModel>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CategoryId { get; set; }
    public string CompanyId { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int Stock { get; set; }
    public int SoldQuantity { get; set; }
    public string MainImage { get; set; }
    public List<string> GalleryImages { get; set; } = new();
}

public class UpdateProductRequestModelValidator : AbstractValidator<UpdateProductRequestModel>
{
    public UpdateProductRequestModelValidator()
    {
        RuleFor(x => x.Id).Required();
        RuleFor(x => x.Name).Required().Max(50);
        RuleFor(x => x.Description).Max(500);
        RuleFor(x => x.CategoryId).Required();
        RuleFor(x => x.CompanyId).Required();
        RuleFor(x => x.Price).Required();
        RuleFor(x => x.Stock).Required();
        RuleFor(x => x.MainImage).Required();
    }
}

public class UpdateProductRequestHandler
    : IRequestHandler<UpdateProductRequestModel, UpdateProductResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly ISessionService _sessionService;
    private readonly IBackgroundTaskQueueService _taskQueueService;
    private readonly IImageService _imageService;

    public UpdateProductRequestHandler(
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

    public async Task<UpdateProductResponseModel> Handle(
        UpdateProductRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var product = await _context.Product
            .Include(p => p.Images) 
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        
        var name = request.Name.ToLower();
        if (name != product.Name)
        {
            var isNameTaken = await _context.Product.ActiveAny(
                u => u.Name == name, 
                cancellationToken
            );
            
            if (isNameTaken) throw new AlreadyExistsException(nameof(name));
            product.Name = name;
        }
        
        if (request.Description != product.Description && request.Description != null)
        {
            product.Description = request.Description;
        }

        if (request.CategoryId != product.CategoryId && request.CategoryId != null)
        {
            var categoryCheck = await _context.Category.ActiveAny(c => c.Id == request.CategoryId);
            if (!categoryCheck) throw new NotFoundException(nameof(request.CategoryId));
            
            product.CategoryId = request.CategoryId;
        }        
        
        if (request.CompanyId != product.CompanyId && request.CompanyId != null)
        {
            var companyCheck = await _context.Company.ActiveAny(c => c.Id == request.CompanyId);
            if (!companyCheck) throw new NotFoundException(nameof(request.CompanyId));
            
            product.CompanyId = request.CompanyId;
        }
        
        if (request.MainImage.IsBase64()) 
        {
            var oldMainImage = product.Images.FirstOrDefault(i => i.IsMain);
            var oldPath = oldMainImage?.Url;

            // Save new image
            var newPath = await _imageService.SaveImageToServer(
                request.MainImage,
                ".png",
                "products"
            );

            if (oldMainImage != null) oldMainImage.Url = newPath;

            else
            {
                product.Images ??= new List<ProductImage>();
                product.Images.Add(new ProductImage { Url = newPath, IsMain = true });
            }

            // Clean up old file in background
            if (!string.IsNullOrEmpty(oldPath))
                _taskQueueService.QueueBackgroundWorkItem((Func<CancellationToken, ValueTask>)
                    (async t => await _imageService.DeleteImageFromServer(oldPath)));
        }
        
        // Handling the Gallery Images
        
        var imagesToRemove = product.Images
            .Where(i => !i.IsMain && !request.GalleryImages.Contains(i.Url))
            .ToList();

        foreach (var img in imagesToRemove)
        {
            var pathToDelete = img.Url;
            product.Images.Remove(img); // Remove from DB collection

            // Queue physical file deletion
            if (pathToDelete.IsNotNullOrWhiteSpace())
            {
                var finalPathToDelete = pathToDelete; 
                _taskQueueService.QueueBackgroundWorkItem((Func<CancellationToken, ValueTask>) 
                        (async token => await _imageService.DeleteImageFromServer(finalPathToDelete)));
            }
        }
        
        var newGalleryImages = request.GalleryImages
            .Where(img => img.IsBase64())
            .ToList();

        foreach (var base64 in newGalleryImages)
        {
            var newPath = await _imageService.SaveImageToServer(
                base64,
                ".png",
                "products"
            );
            
            product.Images.Add(new ProductImage 
            { 
                Url = newPath, 
                IsMain = false 
            });
        }

        product.Price = request.Price;
        product.DiscountPrice = request.DiscountedPrice;
        product.Stock = request.Stock;
        product.SoldQuantity = request.SoldQuantity;
            
        _context.Product.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new UpdateProductResponseModel() { Data = new ProductDto(product) };
    }
}

public class UpdateProductResponseModel
{
    public ProductDto Data { get; set; }
}