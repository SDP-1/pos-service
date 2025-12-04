using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services.Permissions
{
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(UserRole role);
        Task<bool> AddPermissionToRoleAsync(UserRole role, string permissionName);
        Task<bool> RemovePermissionFromRoleAsync(UserRole role, string permissionName);
    }
}