using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class UserRole : IdentityUserRole<string>
    {
        public Role Role { get; set; }
        public User User { get; set; }
    }
}