using Microsoft.AspNetCore.Identity;

namespace IdentityWebApp.Models
{
    public class AppUser : IdentityUser
    {
        public string Picture { get; set; }
        public string City { get; set; }
    }
}
