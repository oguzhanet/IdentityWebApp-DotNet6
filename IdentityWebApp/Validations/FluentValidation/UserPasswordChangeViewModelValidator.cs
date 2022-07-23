using FluentValidation;
using IdentityWebApp.Models.ViewModels;

namespace IdentityWebApp.Validations.FluentValidation
{
    public class UserPasswordChangeViewModelValidator : AbstractValidator<UserPasswordChangeViewModel>
    {
        public UserPasswordChangeViewModelValidator()
        {
            RuleFor(x => x.PasswordOld)
                .NotEmpty()
                .MinimumLength(8);

            RuleFor(x => x.PasswordNew)
                .NotEmpty()
                .MinimumLength(8);

            RuleFor(x => x.PasswordConfirm)
                .NotEmpty()
                .MinimumLength(8);

            RuleFor(x => x).Custom((x, context) =>
            {
                if (x.PasswordNew != x.PasswordConfirm)
                {
                    context.AddFailure(nameof(x.PasswordNew), "Passwords should match");
                }
            });
        }
    }
}
