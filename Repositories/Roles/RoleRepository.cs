using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories.Roles
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all roles in the system.
        /// </summary>
        /// <returns>A collection of Role entities.</returns>
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves roles that are currently active (IsActive == true).
        /// </summary>
        /// <returns>A collection of active Role entities.</returns>
        public async Task<IEnumerable<Role>> GetActiveAsync()
        {
            return await _context.Roles.AsNoTracking().Where(r => r.IsActive).ToListAsync();
        }

        /// <summary>
        /// Finds a role by its database identifier.
        /// </summary>
        /// <param name="id">Database id of the role.</param>
        /// <returns>The Role when found; otherwise null.</returns>
        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        /// <summary>
        /// Finds a role by its unique name.
        /// </summary>
        /// <param name="name">Name of the role to find.</param>
        /// <returns>The Role when found; otherwise null.</returns>
        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == name);
        }

        /// <summary>
        /// Adds a new role to the database and assigns a new UUID.
        /// </summary>
        /// <param name="role">Role entity to add.</param>
        /// <returns>The added Role with updated identity fields.</returns>
        public async Task<Role> AddAsync(Role role)
        {
            role.Uuid = Guid.NewGuid().ToString();
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="role">Role entity with updated values.</param>
        /// <returns>The updated Role.</returns>
        public async Task<Role> UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Deletes the given role from the database.
        /// </summary>
        /// <param name="role">Role entity to remove.</param>
        public async Task DeleteAsync(Role role)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}