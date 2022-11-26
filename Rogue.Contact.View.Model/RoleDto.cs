namespace Rogue.Contacts.View.Model;

public sealed record RoleDto(string Id, string Name, IEnumerable<string> Permissions);
