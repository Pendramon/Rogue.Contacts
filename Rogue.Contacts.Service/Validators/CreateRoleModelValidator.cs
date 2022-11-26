﻿using FluentValidation;
using Rogue.Contacts.View.Model;

namespace Rogue.Contacts.Service.Validators;

public class CreateRoleModelValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleModelValidator()
    {
        RuleFor(m => m.Owner).NotEmpty().Username();
        RuleFor(m => m.Business).NotEmpty().BusinessName();
        RuleFor(m => m.Name).NotEmpty().RoleName();
        RuleFor(m => m.Permissions).ForEach(p => p.IsEnumName(typeof(Permission)));
    }
}
