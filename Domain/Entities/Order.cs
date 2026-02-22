using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Order
{
    public string UserId { get; set; }
    public string ProductId { get; set; }
    public string Quantity { get; set; }
    public DateTime OrderDate { get; set; }
    
    
    
    // Foreign Keys
    [ForeignKey("UserId")]
    public User User { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}