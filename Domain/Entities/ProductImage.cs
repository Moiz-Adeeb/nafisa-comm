using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class ProductImage : Base
{
    public string ProductId { get; set; }
    
    
    // Foreign Keys
    [ForeignKey(("ProductId"))]
    public virtual Product Product { get; set; }
}