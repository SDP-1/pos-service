using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services.Permissions
{
    public interface IPermissionService
    {
        /// <summary>
        /// Retrieves all permissions configured in the system.
        /// </summary>
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();

        /// <summary>
        /// Retrieves permissions assigned to a specific role.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId);

        /// <summary>
        /// Adds a named permission to a role via repository.
        /// </summary>
        Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName);

        /// <summary>
        /// Removes a named permission from a role via repository.
        /// </summary>
        Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName);
    }
}