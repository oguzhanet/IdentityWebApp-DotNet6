using System.ComponentModel.DataAnnotations;

namespace IdentityWebApp.Models.ViewModels
{
    public class UserViewModel
    {
        [Display(Name ="Kullanıcı Adı")]
        public string UserName { get; set; }

        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email Adresi")]
        public string Email { get; set; }

        [Display(Name = "Şifre")]
        public string Password { get; set; }
    }
}
