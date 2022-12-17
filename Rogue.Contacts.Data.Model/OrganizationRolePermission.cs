namespace Rogue.Contacts.Data.Model;

public sealed class OrganizationRolePermission
{
    public int OrganizationRoleId { get; set; }

    public OrganizationRole OrganizationRole { get; set; } = null!;

    public int OrganizationPermissionId { get; set; }

    public OrganizationPermission OrganizationPermission { get; set; } = null!;
}
