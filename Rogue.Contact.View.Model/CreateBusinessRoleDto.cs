namespace Rogue.Contacts.View.Model;

public sealed record CreateBusinessRoleDto(string Owner, string Business, string Name, IEnumerable<string>? Permissions = default);
