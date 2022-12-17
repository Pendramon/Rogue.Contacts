using FluentValidation;
using Rogue.Contacts.Shared;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public sealed class CreateBusinessRoleModelValidator : AbstractValidator<CreateBusinessRoleDto>
{
    public CreateBusinessRoleModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
        RuleFor(m => m.Name).NotEmpty().RoleName();
        RuleFor(m => m.Permissions).ForEach(p => p.IsEnumName(typeof(BusinessPermissionEnum)));
    }
}
