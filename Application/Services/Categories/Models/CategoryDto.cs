using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Application.Shared;
using Domain.Entities;

namespace Application.Services.Categories.Models;

public class CategoryDetailDto : CategoryDto
{
    public int ProductCount { get; set; }
    public int ChildrenCount { get; set; }
}

public class CategoryDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ParentCategoryId { get; set; }
    public List<CategoryDetailDto> Children { get; set; } = new List<CategoryDetailDto>();
    public DateTimeOffset CreatedDate { get; set; }

    public CategoryDto() { }

    public CategoryDto(Category category)
    {
        Name = category.Name;
        Description = category.Description;
        ParentCategoryId = category.ParentCategoryId;
        CreatedDate = category.CreatedDate;
    }
}

public class CategorySelector
{
    public static readonly Expression<Func<Category, CategoryDto>> Selector = p => new CategoryDto()
    {
        Name = p.Name,
        Description = p.Description,
        ParentCategoryId = p.ParentCategoryId,
        CreatedDate = p.CreatedDate,
    };
    public static readonly Expression<Func<Category, CategoryDetailDto>> SelectorDetail =
        p => new CategoryDetailDto()
        {
            Name = p.Name,
            Description =  p.Description,
            ParentCategoryId = p.ParentCategoryId,
            Children = new List<CategoryDetailDto>(),
            CreatedDate = p.CreatedDate,
            ChildrenCount = p.Children.Count(),
            ProductCount = p.Products.Count(),
        };

    public static readonly Expression<Func<Category, DropDownDto<string>>> SelectorDropDown =
        p => new DropDownDto<string>() { Name = p.Name, Id = p.Id };
    
    public static class CategoryTreeBuilder
    {
        public static List<CategoryDetailDto> BuildTree(List<CategoryDetailDto> flatCategories)
        {
            // Lookup for fast parent → children mapping
            var lookup = flatCategories.ToLookup(c => c.ParentCategoryId);

            foreach (var category in flatCategories)
            {
                category.Children = lookup[category.Name].ToList();
            }

            // Return top-level categories
            return flatCategories.Where(c => c.ParentCategoryId == null).ToList();
        }
    }
}

