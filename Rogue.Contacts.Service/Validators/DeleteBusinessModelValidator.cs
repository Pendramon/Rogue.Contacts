using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class DeleteBusinessModelValidator : AbstractValidator<DeleteBusinessRequestDto>
{
    public DeleteBusinessModelValidator()
    {
        RuleFor(m => m.Name).NotEmpty().MaximumLength(255);
    }
}
