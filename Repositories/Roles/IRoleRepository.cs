using pos_service.Models;

namespace pos_service.Repositories.Roles
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<IEnumerable<Role>> GetActiveAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<Role> AddAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task DeleteAsync(Role role);

        /// <summary>
        /// Accumulates permissions for a role and all of its descendant roles recursively down the hierarchy using BFS traversal.
        /// </summary>
        List<Permission> GetPermissionsByRoleId(int roleId);
        Task<List<Permission>> GetPermissionsByRoleIdAsync(int roleId);
    }
}