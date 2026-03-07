using Application.Exceptions;
using Application.Extensions;
using Application.Services.Categories.Models;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Persistence.Context;
using Persistence.Extension;

namespace Application.Services.Categories.Commands;

public class CreateCategoryRequestModel : IRequest<CreateCategoryResponseModel>
{
    public string  Name { get; set; }
    public string  Description { get; set; }
    public string  ParentCategoryId { get; set; }
}

public class CreateCategoryRequestModelValidator : AbstractValidator<CreateCategoryRequestModel>
{
    public CreateCategoryRequestModelValidator()
    {
        RuleFor(x => x.Name).Required().Max(50);
        RuleFor(y => y.Description).Max(500);
    }
}

public class CreateCategoryRequestHandler : IRequestHandler<CreateCategoryRequestModel, CreateCategoryResponseModel>
{
    private readonly ApplicationDbContext _context;

    public CreateCategoryRequestHandler (
        ApplicationDbContext context
    )
    {
        _context = context;
    }

    public async Task<CreateCategoryResponseModel> Handle(
        CreateCategoryRequestModel request,
        CancellationToken cancellationToken
    )
    {
        var name = request.Name;
        var existCheck = await _context.Category
            .ActiveAny(c => c.Name == name, cancellationToken);

        if(existCheck) throw new AlreadyExistsException(nameof(name));
        
        var category = new Category()
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };
        
        await _context.Category.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResponseModel() { Data  = new CategoryDto(category) };
    }
}

public class CreateCategoryResponseModel
{
    public CategoryDto Data { get; set; }
}