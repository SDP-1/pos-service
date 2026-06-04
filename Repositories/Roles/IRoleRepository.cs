using pos_service.Models;

namespace pos_service.Repositories.Roles
{
    public interface IRoleRepository
    {
        /// <summary>
        /// Retrieves all roles in the system.
        /// </summary>
        /// <returns>A collection of Role entities.</returns>
        Task<IEnumerable<Role>> GetAllAsync();

        /// <summary>
        /// Retrieves roles that are currently active (IsActive == true).
        /// </summary>
        /// <returns>A collection of active Role entities.</returns>
        Task<IEnumerable<Role>> GetActiveAsync();

        /// <summary>
        /// Finds a role by its database identifier.
        /// </summary>
        /// <param name="id">Database id of the role.</param>
        /// <returns>The Role when found; otherwise null.</returns>
        Task<Role?> GetByIdAsync(int id);

        /// <summary>
        /// Finds a role by its unique name.
        /// </summary>
        /// <param name="name">Name of the role to find.</param>
        /// <returns>The Role when found; otherwise null.</returns>
        Task<Role?> GetByNameAsync(string name);

        /// <summary>
        /// Adds a new role to the database and assigns a new UUID.
        /// </summary>
        /// <param name="role">Role entity to add.</param>
        /// <returns>The added Role with updated identity fields.</returns>
        Task<Role> AddAsync(Role role);

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        /// <param name="role">Role entity with updated values.</param>
        /// <returns>The updated Role.</returns>
        Task<Role> UpdateAsync(Role role);

        /// <summary>
        /// Deletes the given role from the database.
        /// </summary>
        /// <param name="role">Role entity to remove.</param>
        Task DeleteAsync(Role role);
    }
}