using Rogue.Contacts.Shared;

namespace Rogue.Contacts.Data.Model;

public sealed class OrganizationPermission
{
    private OrganizationPermission(OrganizationPermissionEnum permission)
    {
        Id = (int)permission;
        Name = permission.ToString();
    }

#pragma warning disable IDE0051 // Used by EF to instantiate a permission.
    private OrganizationPermission(string name)
    {
        Name = name;
    }
#pragma warning restore IDE0051

    public int Id { get; }

    public string Name { get; }

    public ICollection<OrganizationRolePermission> RolePermissions { get; set; } = null!;

    public static implicit operator OrganizationPermission(OrganizationPermissionEnum permission) =>
        new(permission);

    public static implicit operator OrganizationPermissionEnum(OrganizationPermission organizationPermission) =>
        (OrganizationPermissionEnum)organizationPermission.Id;
}
