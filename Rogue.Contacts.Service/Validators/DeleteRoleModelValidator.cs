using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public class DeleteRoleModelValidator : AbstractValidator<DeleteRoleDto>
{
    public DeleteRoleModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
        RuleFor(m => m.RoleId).NotEmpty().Id();
    }
}
