using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Product : Base
{
    public string Name { get; set; }
    public string Picture { get; set; }
    public string Description { get; set; } 
    
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    
    public int Stock { get; set; }
    public int SoldQuantity { get; set; }

    public bool IsActive { get; set; } = true;
    
    public string CategoryId { get; set; }
    
    // Foreign Keys
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }
    
    // Collections
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}