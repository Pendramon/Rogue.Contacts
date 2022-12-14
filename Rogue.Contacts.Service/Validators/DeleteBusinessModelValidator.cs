using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class DeleteBusinessModelValidator : AbstractValidator<DeleteBusinessDto>
{
    public DeleteBusinessModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
    }
}
