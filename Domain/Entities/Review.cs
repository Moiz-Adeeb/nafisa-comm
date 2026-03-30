using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Review : Base
{
    public string ProductReview { get; set; }
    public decimal Rating { get; set; }
    public string ProductId { get; set; }
    public string UserId { get; set; }

    // Foreign Keys
    [ForeignKey("UserId")] public virtual User User { get; set; }
    [ForeignKey("ProductId")] public virtual Product Product { get; set; }

}