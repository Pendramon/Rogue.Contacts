using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class CreateBusinessModelValidator : AbstractValidator<CreateBusinessDto>
{
    public CreateBusinessModelValidator()
    {
        RuleFor(m => m.Name).NotEmpty().BusinessName();
    }
}
