namespace Rogue.Contacts.Data.Model;

public sealed class Business
{
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public Party Owner { get; set; } = null!;

    required public string Name { get; set; }

    required public DateTime CreatedAt { get; set; }

    public ICollection<BusinessRole> BusinessRoles { get; set; } = null!;
}
