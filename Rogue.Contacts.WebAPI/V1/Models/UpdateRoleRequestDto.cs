namespace Rogue.Contacts.WebAPI.V1.Models;

public sealed record UpdateRoleRequestDto(string Name, string[] Permissions);
