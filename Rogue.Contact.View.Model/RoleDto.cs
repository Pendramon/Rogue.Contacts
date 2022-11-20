using Rogue.Contacts.Shared.Models;

namespace Rogue.Contacts.View.Model;

public record RoleDto(string Name, Permission[] Permissions);
