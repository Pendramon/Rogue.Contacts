namespace Rogue.Contacts.WebAPI.V1.Models;

public sealed record CreateBusinessRoleRequestDto(string Name, IEnumerable<string>? Permissions = default);