namespace Rogue.Contacts.View.Model;

public sealed record BusinessDto(string Owner, string Name, DateTime CreatedAt, IEnumerable<RoleDto> Roles);
