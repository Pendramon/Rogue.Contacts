namespace Rogue.Contacts.View.Model;

public sealed record UpdateRoleDto(string Owner, string Business, string RoleId, string? Name, IEnumerable<string>? Permissions);
