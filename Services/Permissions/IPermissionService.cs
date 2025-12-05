using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services.Permissions
{
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId);
        Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName);
    }
}