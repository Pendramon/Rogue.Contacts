using System.Security.Claims;
using Rogue.Contacts.Service.Interfaces;

namespace Rogue.Contacts.WebAPI
{
    public sealed class UserResolver : IUserResolver
    {
        private readonly IHttpContextAccessor context;

        public UserResolver(IHttpContextAccessor context)
        {
            this.context = context;
        }

        public int GetUserId()
        {
            var id = this.context.HttpContext!.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

            // TODO: Ensure this method will always work on controller methods with Authorize attribute.
            return int.Parse(id);
        }
    }
}
