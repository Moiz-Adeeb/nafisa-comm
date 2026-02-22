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
    public string Status { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public string ProfilePicture { get; set; }
    public DateTimeOffset LastSeen { get; set; }

    //Foreign Keys
    public ICollection<Conversation> ChatsAsUser1 { get; set; }
    public ICollection<Conversation> ChatsAsUser2 { get; set; }
    public ICollection<Messages> SentMessages { get; set; }
    public ICollection<Messages> ReceivedMessages { get; set; }    
}