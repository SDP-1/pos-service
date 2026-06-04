using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pos_service.Models;
using pos_service.Repositories.Permissions;

namespace pos_service.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repo;

        public PermissionService(IPermissionRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Retrieves all permissions configured in the system.
        /// </summary>
        public Task<IEnumerable<Permission>> GetAllPermissionsAsync()
            => _repo.GetAllAsync();

        /// <summary>
        /// Retrieves permissions assigned to a specific role.
        /// </summary>
        /// <param name="roleId">Database id of the role.</param>
        public Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
            => _repo.GetForRoleAsync(roleId);

        /// <summary>
        /// Adds a named permission to a role via repository.
        /// </summary>
        public Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName)
            => _repo.AddPermissionToRoleAsync(roleId, permissionName);

        /// <summary>
        /// Removes a named permission from a role via repository.
        /// </summary>
        public Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName)
            => _repo.RemovePermissionFromRoleAsync(roleId, permissionName);
    }
}
