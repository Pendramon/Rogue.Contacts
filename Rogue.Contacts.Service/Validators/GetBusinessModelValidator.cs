using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class GetBusinessModelValidator : AbstractValidator<GetBusinessDto>
{
    public GetBusinessModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Name).NotEmpty().BusinessName();
    }
}
