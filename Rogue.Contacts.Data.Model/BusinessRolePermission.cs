namespace Rogue.Contacts.Data.Model;

public sealed class BusinessRolePermission
{
    public int BusinessRoleId { get; set; }

    public BusinessRole BusinessRole { get; set; } = null!;

    public int BusinessPermissionId { get; set; }

    public BusinessPermission BusinessPermission { get; set; } = null!;
}
