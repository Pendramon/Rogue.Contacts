namespace Rogue.Contacts.Shared;

public enum OrganizationPermissionEnum
{
    /// <summary>
    /// The ability to view an organization and its businesses.
    /// </summary>
    ViewOrganization = 1,

    /// <summary>
    /// Ability to create, update, delete and assign user roles.
    /// </summary>
    ManageRoles = 2,

    /// <summary>
    /// The ability to create, delete and manage businesses for an organization.
    /// </summary>
    ManageBusinesses = 3,


}
