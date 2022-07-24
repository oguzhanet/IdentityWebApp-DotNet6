using IdentityWebApp.Enums;

namespace IdentityWebApp.Models.ViewModels
{
    public class UserViewModel
    {
        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string City { get; set; }

        public string Picture { get; set; }

        public GenderEnum Gender { get; set; } = GenderEnum.Unspecified; 

        public DateTime? BirthDate { get; set; }
    }
}
