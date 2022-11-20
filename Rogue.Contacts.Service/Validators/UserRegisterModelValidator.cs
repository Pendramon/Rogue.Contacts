using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class UserRegisterModelValidator : AbstractValidator<UserRegisterRequestDto>
{
    public UserRegisterModelValidator()
    {
        RuleFor(m => m.Username).NotEmpty().MinimumLength(3).MaximumLength(20).Matches(@"^[A-Za-z0-9]+(?:[-_.][A-Za-z0-9]+)*$").WithMessage("Usernames can only consist of alphanumerical characters. You may also use hyphens, underscores and dots as separators.");

        RuleFor(m => m.Email).NotEmpty().EmailAddress();

        RuleFor(m => m.Password).NotEmpty().MinimumLength(8).MaximumLength(256).Matches(@"^[!-~]*$").WithMessage("Passwords can only consist of alphanumerical and special characters. ASCII Range 33 to 126.");
    }
}