using FluentValidation;
using Rogue.Contacts.Shared;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class UpdateBusinessRoleModelValidator : AbstractValidator<UpdateBusinessRoleDto>
{
    public UpdateBusinessRoleModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
        RuleFor(m => m.RoleId).NotEmpty();
        RuleFor(m => m.Permissions).ForEach(p => p.IsEnumName(typeof(BusinessPermissionEnum)));
        RuleFor(m => m.Name).RoleName();
    }
}
