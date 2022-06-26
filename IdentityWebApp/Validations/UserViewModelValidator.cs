using FluentValidation;
using IdentityWebApp.Models.ViewModels;

namespace IdentityWebApp.Validations
{
    public class UserViewModelValidator:AbstractValidator<UserViewModel>
    {
        public UserViewModelValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
