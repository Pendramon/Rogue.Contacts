namespace Rogue.Contacts.Data.Model;

public abstract class Party
{
    public int Id { get; set; }

    required public string Name { get; set; }

    required public DateTime CreatedAt { get; set; }
}
