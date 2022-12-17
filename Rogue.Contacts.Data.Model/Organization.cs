namespace Rogue.Contacts.Data.Model;

public sealed class Organization : Party
{
    public int OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    public ICollection<OrganizationRole> OrganizationRoles { get; set; } = null!;
}
