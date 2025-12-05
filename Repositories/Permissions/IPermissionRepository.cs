using pos_service.Models;

namespace pos_service.Repositories.Permissions
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<IEnumerable<Permission>> GetForRoleAsync(int roleId);
        Task<bool> AddPermissionToRoleAsync(int roleId, string permissionName);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, string permissionName);
    }
}
