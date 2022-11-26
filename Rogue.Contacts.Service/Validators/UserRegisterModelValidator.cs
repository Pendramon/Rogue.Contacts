using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class UserRegisterModelValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterModelValidator()
    {
        RuleFor(m => m.Username).NotEmpty().Username();

        RuleFor(m => m.Email).NotEmpty().EmailAddress();

        RuleFor(m => m.Password).NotEmpty().Password();
    }
}