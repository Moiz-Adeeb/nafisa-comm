using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Cart : Base
{
    public string ProductId { get; set; }
    public string UserId { get; set; }
    public int Quantity { get; set; }

    // Foreign Keys
    [ForeignKey("UserId")] public virtual User User { get; set; }
    [ForeignKey("ProductId")] public virtual Product Product { get; set; }
}