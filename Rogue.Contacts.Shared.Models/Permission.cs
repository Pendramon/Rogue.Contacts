namespace Rogue.Contacts.Shared.Models;

public enum Permission
{
    /// <summary>
    /// Ability to create, update and delete user roles.
    /// </summary>
    ManageRoles = 0,

    /// <summary>
    /// Ability to rename business name.
    /// </summary>
    ManageBusiness = 1,

    /// <summary>
    /// Ability to see business and confidential client information.
    /// </summary>
    ViewBusiness = 2,
}
