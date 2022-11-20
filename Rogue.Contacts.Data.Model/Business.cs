using MongoDB.Bson;

namespace Rogue.Contacts.Data.Model;

public sealed class Business
{
    public Business(string name, ObjectId ownerId, string ownerUsername, DateTime createdAt)
    {
        Name = name;
        OwnerId = ownerId;
        OwnerUsername = ownerUsername;
        CreatedAt = createdAt;
    }

    public ObjectId Id { get; set; }

    public string Name { get; set; }

    public ObjectId OwnerId { get; set; }

    public string OwnerUsername { get; set; }

    public DateTime CreatedAt { get; set; }
}
