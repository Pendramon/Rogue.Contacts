namespace Rogue.Contacts.Data.Model;

public sealed class OrganizationRole
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    required public string Name { get; set; }

    public ICollection<OrganizationRolePermission> OrganizationRolePermissions { get; set; } = null!;

    public ICollection<UserOrganizationRole> UserOrganizationRoles { get; set; } = null!;
}