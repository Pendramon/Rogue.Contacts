namespace Rogue.Contacts.Data.Model;

public sealed class UserBusinessRole
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int BusinessRoleId { get; set; }

    public BusinessRole BusinessRole { get; set; } = null!;
}
