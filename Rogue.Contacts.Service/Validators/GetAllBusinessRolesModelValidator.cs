using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class GetAllBusinessRolesModelValidator : AbstractValidator<GetAllBusinessRolesDto>
{
    public GetAllBusinessRolesModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.BusinessName).NotEmpty().BusinessName();
    }
}
