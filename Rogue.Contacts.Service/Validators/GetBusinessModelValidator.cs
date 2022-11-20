using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class GetBusinessModelValidator : AbstractValidator<GetBusinessRequestDto>
{
    public GetBusinessModelValidator()
    {
        RuleFor(m => m.OwnerUsername).MinimumLength(3).MaximumLength(20);
        RuleFor(m => m.BusinessName).MaximumLength(255);
    }
}
