using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

[Table("Users")]

public class User : IdentityUser, IBase
{
    public User() { }
    
    public string Name { get; set; }
    public string ProfilePicture { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
    
    public string DeliveryAddress { get; set; }
    public string City { get; set; }
    public int? PostalCode { get; set; }

    //Foreign Keys
    public virtual ICollection<Order> Orders { get; set; }  
}