using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class OrderItem : Base
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Foreign Keys
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }
}