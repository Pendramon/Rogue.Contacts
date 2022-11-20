using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Rogue.Contacts.Data.Model;

namespace Rogue.Contacts.Data;

public sealed class MongoContext
{
    public MongoContext(IConfiguration configuration)
    {
        var pack = new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String),
        };
        ConventionRegistry.Register("EnumStringConversion", pack, t => true);
        Client = new MongoClient(configuration.GetConnectionString("RogueContacts"));
    }

    public IMongoCollection<User> Users => ContactsDb.GetCollection<User>("User");

    public IMongoCollection<Business> Businesses => ContactsDb.GetCollection<Business>("Business");

    private IMongoClient Client { get; }

    private IMongoDatabase ContactsDb => Client.GetDatabase("RogueContacts");
}
