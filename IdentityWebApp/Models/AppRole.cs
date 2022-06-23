using Microsoft.AspNetCore.Identity;

namespace IdentityWebApp.Models
{
    public class AppRole : IdentityRole
    {
        public string RoleType { get; set; }
    }
}
