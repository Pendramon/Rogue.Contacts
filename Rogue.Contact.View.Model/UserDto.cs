namespace Rogue.Contacts.View.Model;

public sealed record UserDto(string Username, string DisplayName, string Email, DateTime CreatedAt, IEnumerable<string> Roles);