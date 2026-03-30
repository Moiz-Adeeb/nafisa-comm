using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

[Table("users")]

public class User : IdentityUser, IBase
{
    public User() { }
    
    public string Name { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public string DeliveryAddress { get; set; }
    public string City { get; set; }
    public int? PostalCode { get; set; }
    public string Image { get; set; }
    public bool IsEnabled { get; set; }
    
    
    /// <summary>
    /// Navigation property for the roles this user belongs to.
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; }
    
    /// <summary>
    /// Navigation property for the claims this user possesses.
    /// </summary>
    public virtual ICollection<UserClaim> UserClaims { get; set; }
    
    // Navigation Properties 
    public IEnumerable<UserSetting> UserSettings { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Cart> Cart { get; set; } = new List<Cart>();
    public virtual ICollection<WishList> WishList { get; set; } = new List<WishList>();
}
