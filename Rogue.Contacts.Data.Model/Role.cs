using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rogue.Contacts.Data.Model;

public sealed class Role
{
    public Role(string name, string[]? permissions = default)
    {
        this.Id = ObjectId.GenerateNewId();
        this.Name = name;
        this.Permissions = permissions ?? Array.Empty<string>();
    }

    [BsonId]
    public ObjectId Id { get; set; }

    public string Name { get; set; }

    public string[] Permissions { get; set; }
}
