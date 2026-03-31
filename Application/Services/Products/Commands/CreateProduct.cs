using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces;
using Application.Services.Products.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Products.Commands;

public class CreateProductRequestModel : IRequest<CreateProductResponseModel>
{
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

public class CreateProductRequestModelValidator : AbstractValidator<CreateProductRequestModel>
{
    public CreateProductRequestModelValidator()
    {
        RuleFor(x => x.Name).Required().Max(50);
        RuleFor(x => x.Description).Max(500);
        RuleFor(x => x.CategoryId).Required();
        RuleFor(x => x.CompanyId).Required();
        RuleFor(x => x.Price).Required();
        RuleFor(x => x.Stock).Required();
        RuleFor(x => x.MainImage).Required();
    }
}

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequestModel, CreateProductResponseModel>
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;

    public CreateProductRequestHandler (
        ApplicationDbContext context,
        IImageService imageService
    )
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<CreateProductResponseModel> Handle(
        CreateProductRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var name = request.Name;
        var existCheck = await _context.Product
            .ActiveAny(p => p.Name == name, cancellationToken);
        if(existCheck) throw new AlreadyExistsException(nameof(name));
        
        var categoryCheck = await _context.Category
            .ActiveAny(c => c.Id == request.CategoryId, cancellationToken);
        if (categoryCheck) throw new NotFoundException(nameof(Category));

        var companyCheck = await _context.Company
            .ActiveAny(c => c.Id == request.CompanyId, cancellationToken);
        if (companyCheck) throw new NotFoundException(nameof(Company));
        
        var productImages = new List<ProductImage>();
        var mainImagePath = await _imageService.SaveImageToServer(
            request.MainImage, 
            ".png",
            "products"
        );
        
        productImages.Add(new ProductImage 
        { 
            Url = mainImagePath, 
            IsMain = true 
        });
        
        if (request.GalleryImages != null && request.GalleryImages.Any())
        {
            foreach (var base64Image in request.GalleryImages)
            {
                var path = await _imageService.SaveImageToServer(base64Image, ".png", "products");
                productImages.Add(new ProductImage 
                { 
                    Url = path, 
                    IsMain = false
                });
            }
        }
        
        var product = new Product()
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            CompanyId = request.CompanyId,
            Price = request.Price,
            DiscountPrice = request.DiscountedPrice,
            Stock = request.Stock,
            SoldQuantity = request.SoldQuantity,
            IsActive = true,
            Images = productImages
        };
        
        await _context.Product.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateProductResponseModel() { Data  = new ProductDto(product) };
    }
}

public class CreateProductResponseModel
{
    public ProductDto Data { get; set; }
}