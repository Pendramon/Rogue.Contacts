using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class DeleteBusinessRoleModelValidator : AbstractValidator<DeleteBusinessRoleDto>
{
    public DeleteBusinessRoleModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
        RuleFor(m => m.RoleId).NotEmpty();
    }
}
