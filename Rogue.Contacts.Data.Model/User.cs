namespace Rogue.Contacts.Data.Model;

public sealed class User : Party
{
    required public string DisplayName { get; set; }

    required public string Email { get; set; }

    required public string PasswordHash { get; set; }

    public ICollection<UserBusinessRole> UserBusinessRoles { get; set; } = null!;

    public ICollection<UserOrganizationRole> UserOrganizationRoles { get; set; } = null!;
}
