namespace Rogue.Contacts.View.Model;

public sealed record CreateRoleDto(string Owner, string Business, string Name, IEnumerable<string>? Permissions = default);
