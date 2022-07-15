using FluentValidation;
using IdentityWebApp.Models.ViewModels;

namespace IdentityWebApp.Validations.FluentValidation
{
    public class ResetPasswordViewModelValidator:AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordViewModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}
