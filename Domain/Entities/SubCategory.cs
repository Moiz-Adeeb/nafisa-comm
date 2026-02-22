using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class SubCategory
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string CategoryId { get; set; }
    public int TotalProducts { get; set; }
    
    // Foreign Keys
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
    
    // Collections
    public ICollection<Product> Products { get; set; }
}