namespace Rogue.Contacts.View.Model;

public sealed record AddPermissionsToRoleRequestDto(string Owner, string Business, string Role, IEnumerable<string> Permissions);
