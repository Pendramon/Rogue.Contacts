namespace Rogue.Contacts.WebAPI.V1.Models;

public sealed record UpdateBusinessRoleRequestDto(string? Name, IEnumerable<string>? Permissions);
