using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class Order : Base
{
    // Human-Readable Number Code
    public string OrderNumber { get; set; }
    
    public string UserId { get; set; }
    
    public string DeliveryAddress { get; set; }
    public string City { get; set; }
    public int? PostalCode { get; set; }
    public string PhoneNumber { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public OrderStatus Status { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    
    // Foreign Keys
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    
    // Collections
    public virtual ICollection<OrderItem> OrderItems { get; set; }

}