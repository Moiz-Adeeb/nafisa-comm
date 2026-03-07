using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class ProductImage : Base
{
    public string ProductId { get; set; }

    public string Url { get; set; }

    public bool IsMain { get; set; } = false;
    
    
    // Foreign Keys
    [ForeignKey(("ProductId"))]
    public virtual Product Product { get; set; }
}