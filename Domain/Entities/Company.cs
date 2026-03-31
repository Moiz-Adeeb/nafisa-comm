using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Company : Base
{
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // Collections
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    
}