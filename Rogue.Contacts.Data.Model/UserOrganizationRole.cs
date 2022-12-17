namespace Rogue.Contacts.Data.Model;

public sealed class UserOrganizationRole
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int OrganizationRoleId { get; set; }

    public OrganizationRole OrganizationRole { get; set; } = null!;
}
