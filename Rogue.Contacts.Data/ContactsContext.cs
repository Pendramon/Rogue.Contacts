using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rogue.Contacts.Data.Model;

namespace Rogue.Contacts.Data;

public class ContactsContext : DbContext
{
    private readonly IConfiguration configuration;

    public ContactsContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Business> Businesses => Set<Business>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(configuration["RogueContacts:ConnectionString"]);
}
