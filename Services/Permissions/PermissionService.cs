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

        public Task<IEnumerable<Permission>> GetAllPermissionsAsync()
            => _repo.GetAllAsync();

        public Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
            => _repo.GetForRoleAsync(roleId);

        public Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName)
            => _repo.AddPermissionToRoleAsync(roleId, permissionName);

        public Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName)
            => _repo.RemovePermissionFromRoleAsync(roleId, permissionName);
    }
}
