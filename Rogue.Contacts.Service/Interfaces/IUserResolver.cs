using MongoDB.Bson;

namespace Rogue.Contacts.Service.Interfaces
{
    public interface IUserResolver
    {
        public ObjectId GetUserId();
    }
}