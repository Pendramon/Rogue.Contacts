using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Rogue.Contacts.Shared.Models;

namespace Rogue.Contacts.Data.Model;

public sealed record Role(string Name, [property: BsonRepresentation(BsonType.String)] Permission[] Permissions);
