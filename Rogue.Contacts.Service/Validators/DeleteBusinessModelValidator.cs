using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class DeleteBusinessModelValidator : AbstractValidator<DeleteBusinessRequestDto>
{
    public DeleteBusinessModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().MinimumLength(3).MaximumLength(20).Matches(@"^[A-Za-z0-9]+(?:[-_.][A-Za-z0-9]+)*$").WithMessage("Usernames can only consist of alphanumerical characters. You may also use hyphens, underscores and dots as separators.");
        RuleFor(m => m.Business).NotEmpty().MaximumLength(255);
    }
}
