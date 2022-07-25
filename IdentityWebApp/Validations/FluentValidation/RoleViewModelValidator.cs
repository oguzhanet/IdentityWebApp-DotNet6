using FluentValidation;
using IdentityWebApp.Models.ViewModels;

namespace IdentityWebApp.Validations.FluentValidation
{
    public class RoleViewModelValidator:AbstractValidator<RoleViewModel>
    {
        public RoleViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();
        }
    }
}
