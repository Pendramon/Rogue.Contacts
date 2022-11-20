using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class CreateBusinessModelValidator : AbstractValidator<CreateBusinessRequestDto>
{
    public CreateBusinessModelValidator()
    {
        RuleFor(m => m.BusinessName).NotEmpty().MaximumLength(255);
    }
}
