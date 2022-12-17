namespace Rogue.Contacts.View.Model;

public sealed record BusinessRoleDto(int Id, string Name, IEnumerable<string> Permissions);
