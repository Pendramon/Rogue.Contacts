using Rogue.Contacts.Shared;

namespace Rogue.Contacts.Data.Model;

public sealed class BusinessPermission
{
    private BusinessPermission(BusinessPermissionEnum permission)
    {
        Id = (int)permission;
        Name = permission.ToString();
    }

#pragma warning disable IDE0051 // Used by EF to instantiate a permission.
    private BusinessPermission(string name)
    {
        Name = name;
    }
#pragma warning restore IDE0051

    public int Id { get; set; }

    public string Name { get; set; }

    public ICollection<BusinessRolePermission> RolePermissions { get; set; } = null!;

    public static implicit operator BusinessPermission(BusinessPermissionEnum permission) =>
        new(permission);

    public static implicit operator BusinessPermissionEnum(BusinessPermission businessPermission) =>
        (BusinessPermissionEnum)businessPermission.Id;
}
