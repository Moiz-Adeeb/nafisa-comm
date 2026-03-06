using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class RoleClaim : IdentityRoleClaim<string>
    {
        public Role Role { get; set; }
    }
}