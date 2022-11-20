namespace Rogue.Contacts.View.Model;

public sealed record UserRegisterRequestDto(string Username, string DisplayName, string Email, string Password);