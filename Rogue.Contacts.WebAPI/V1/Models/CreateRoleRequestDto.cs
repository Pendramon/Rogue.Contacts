namespace Rogue.Contacts.WebAPI.V1.Models;

public sealed record CreateRoleRequestDto(string Name, string[]? Permissions = default);