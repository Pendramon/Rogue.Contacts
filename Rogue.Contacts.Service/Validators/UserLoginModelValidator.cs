using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class UserLoginModelValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginModelValidator()
    {
        RuleFor(m => m.UsernameOrEmail).NotEmpty();
        RuleFor(m => m.Password).NotEmpty();
    }
}
