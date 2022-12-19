using Microsoft.EntityFrameworkCore;
using Rogue.Contacts.Data;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.Shared;

namespace Rogue.Contacts.Service;

// TODO: Come up with a better way of handling permission check.
public sealed class BusinessAuthorizationService
{
    private readonly ContactsContext context;
    private readonly IUserResolver userResolver;

    public BusinessAuthorizationService(ContactsContext context, IUserResolver userResolver)
    {
        this.context = context;
        this.userResolver = userResolver;
    }

    public async Task<IEnumerable<BusinessPermissionEnum>> GetPermissionsAsync(string ownerName, string businessName, CancellationToken ct = default)
    {
        var businessPermissions = await context.UserBusinessRoles.Select(ur => new
        {
            ur.UserId,
            ur.User.Name,
            Permissions = ur.BusinessRole.BusinessRolePermissions
                  .Select(rp => rp.BusinessPermissionId),
            BusinessName = ur.BusinessRole.Name,
        }).Where(x =>
            x.UserId == userResolver.GetUserId() &&
            x.Name == ownerName &&
            x.BusinessName == businessName)
            .SelectMany(x => x.Permissions)
            .Distinct()
            .Select(p => (BusinessPermissionEnum)p)
            .ToListAsync(ct);

        return businessPermissions;
    }
}
