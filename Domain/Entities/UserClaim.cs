using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class UserClaim : IdentityUserClaim<string>
    {
        public User User { get; set; }
    }
}