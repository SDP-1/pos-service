using pos_service.Models;

namespace pos_service.Repositories.Roles
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<Role> AddAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task DeleteAsync(Role role);
    }
}