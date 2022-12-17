using Microsoft.EntityFrameworkCore;
using Rogue.Contacts.Data;
using Rogue.Contacts.Service.Interfaces;
using Rogue.Contacts.Shared;

namespace Rogue.Contacts.Service;

public class OrganizationAuthorizationService
{
    private readonly ContactsContext context;
    private readonly IUserResolver userResolver;

    public OrganizationAuthorizationService(ContactsContext context, IUserResolver userResolver)
    {
        this.context = context;
        this.userResolver = userResolver;
    }

    public async Task<IEnumerable<OrganizationPermissionEnum>> GetPermissionsAsync(string ownerName, string businessName, CancellationToken ct = default)
    {
        var organizationPermissions = await context.UserOrganizationRoles.Select(ur => new
            {
                ur.UserId,
                ur.User.Name,
                Permissions = ur.OrganizationRole.OrganizationRolePermissions
                    .Select(rp => rp.OrganizationPermissionId),
                Business = ur.OrganizationRole.Name,
            }).Where(x =>
                x.UserId == userResolver.GetUserId() &&
                x.Name == ownerName &&
                x.Business == businessName)
            .SelectMany(x => x.Permissions)
            .Distinct()
            .Select(p => (OrganizationPermissionEnum)p)
            .ToListAsync(ct);

        return organizationPermissions;
    }
}
