using pos_service.Models;
using pos_service.Repositories.Roles;

namespace pos_service.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repo;

        public RoleService(IRoleRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Role>> GetAllAsync()
            => _repo.GetAllAsync();

        public Task<Role?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public async Task<Role?> CreateAsync(Role role)
        {
            // prevent creation of SystemAdmin with id 1 and name
            if (string.Equals(role.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase) || role.Id == 1)
                return null;

            var exists = await _repo.GetByNameAsync(role.Name);
            if (exists != null) return null;

            role.Uuid = Guid.NewGuid().ToString();
            return await _repo.AddAsync(role);
        }

        public async Task<Role?> UpdateAsync(int id, Role role)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            // protect SystemAdmin
            if (existing.Id == 1) return null;  // don't allow updating SystemAdmin

            existing.Name = role.Name;
            existing.Description = role.Description;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            if (existing.Id == 1) return false; // don't allow deleting SystemAdmin

            await _repo.DeleteAsync(existing);
            return true;
        }
    }
}