using pos_service.Models;

namespace pos_service.Repositories.Permissions
{
    public interface IPermissionRepository
    {
        /// <summary>
        /// Retrieves all permissions configured in the system.
        /// </summary>
        /// <returns>A collection of Permission entities.</returns>
        Task<IEnumerable<Permission>> GetAllAsync();

        /// <summary>
        /// Retrieves permissions that are assigned to a specific role.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        /// <returns>A collection of Permission entities assigned to the role.</returns>
        Task<IEnumerable<Permission>> GetForRoleAsync(int roleId);

        /// <summary>
        /// Adds the named permission to the specified role. If the permission record does not exist it will be created.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        /// <param name="permissionName">The name of the permission (matches PermissionType enum value).</param>
        /// <returns>True if the permission was added to the role; false if the mapping already existed.</returns>
        /// <exception cref="ArgumentException">Thrown when permissionName is not a valid PermissionType.</exception>
        Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName);

        /// <summary>
        /// Removes the named permission from the specified role mapping.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        /// <param name="permissionName">The name of the permission (matches PermissionType enum value).</param>
        /// <returns>True when the mapping was removed; false if the permission or mapping was not found or the name is invalid.</returns>
        Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName);
    }
}
