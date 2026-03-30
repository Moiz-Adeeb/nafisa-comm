using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Category : Base
{
    public string Name { get; set; }
    public string? Description { get; set; }
    
    //Optional Parent Category if the Category is a Sub-Category
    public string? ParentCategoryId { get; set; }
    
    //ForeignKeys
    [ForeignKey("ParentCategoryId")]
    public virtual Category? ParentCategory { get; set; }
    
    // Collections
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
}
