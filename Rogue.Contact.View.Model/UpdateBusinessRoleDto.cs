namespace Rogue.Contacts.View.Model;

public sealed record UpdateBusinessRoleDto(string Owner, string Business, int RoleId, string? Name, IEnumerable<string>? Permissions);
