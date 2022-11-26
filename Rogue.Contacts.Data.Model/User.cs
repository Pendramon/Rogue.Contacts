using MongoDB.Bson;

namespace Rogue.Contacts.Data.Model;

public sealed class User
{
    public User(string username, string displayName, string email, string passwordHash, DateTime createdAt, ObjectId[]? roles = default)
    {
        Username = username;
        DisplayName = displayName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
        Roles = roles ?? Array.Empty<ObjectId>();
    }

    public ObjectId Id { get; set; }

    public string Username { get; set; }

    public string DisplayName { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; }

    public ObjectId[] Roles { get; set; }
}
