namespace Rogue.Contacts.Data.Model;

public sealed class BusinessRole
{
    public int Id { get; set; }

    public int BusinessId { get; set; }

    public Business Business { get; set; } = null!;

    required public string Name { get; set; }

    public ICollection<BusinessRolePermission> BusinessRolePermissions { get; set; } = null!;

    public ICollection<UserBusinessRole> UserBusinessRoles { get; set; } = null!;
}
