using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public class UpdateRoleModelValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
        RuleFor(m => m.RoleId).NotEmpty().Id();
        RuleFor(m => m.Permissions).ForEach(p => p.IsEnumName(typeof(Permission)));
        RuleFor(m => m.Name).RoleName();
    }
}
