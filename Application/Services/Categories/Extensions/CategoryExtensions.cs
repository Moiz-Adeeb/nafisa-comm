using Application.Services.Categories.Models;

namespace Application.Services.Categories.Extensions;

public static class CategoryExtensions
{
    public static void GetAllChildIds(CategoryDetailDto category, List<string> idList)
    {
        if (category == null) return;
        idList.Add(category.Id);
        category.Children?.ForEach(child => GetAllChildIds(child, idList)); 
    }
}
