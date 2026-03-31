using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Product : Base
{
    public string Name { get; set; }
    public string Description { get; set; } 
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public int SoldQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public string CategoryId { get; set; }
    public string CompanyId { get; set; }
    public decimal Rating { get; set; } = 0;
    
    // Foreign Keys
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }
    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; }
    
    // Collections
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public uint RowVersion { get; set; }
}
