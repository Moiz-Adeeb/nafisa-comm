namespace Domain.Entities;

public class Category
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int TotalSubCategoryCount { get; set; }
    public int TotalProductCount { get; set; }
    
    // Collections
    public ICollection<SubCategory> SubCategories { get; set; }
    public ICollection<Product> Products { get; set; }
}