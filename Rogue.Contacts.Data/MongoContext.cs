using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Rogue.Contacts.Data.Model;

namespace Rogue.Contacts.Data;

public sealed class MongoContext
{
    public MongoContext(IConfiguration configuration)
    {
        Client = new MongoClient(configuration.GetConnectionString("RogueContacts"));
    }

    public IMongoCollection<User> Users => ContactsDb.GetCollection<User>("User");

    public IMongoCollection<Business> Businesses => ContactsDb.GetCollection<Business>("Business");

    private IMongoClient Client { get; }

    private IMongoDatabase ContactsDb => Client.GetDatabase("RogueContacts");
}
