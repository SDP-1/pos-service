using pos_service.Models;
using pos_service.Models.DTO.Roles;

namespace pos_service.Services.Roles
{
    public interface IRoleService
    {
        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        Task<IEnumerable<RoleResDto>> GetAllAsync();

        /// <summary>
        /// Retrieves only active roles.
        /// </summary>
        Task<IEnumerable<RoleResDto>> GetActiveAsync();

        /// <summary>
        /// Retrieves a role by id.
        /// </summary>
        Task<RoleResDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new role. Creation of the protected SystemAdmin role (id=1 or name SystemAdmin) is not allowed.
        /// </summary>
        Task<RoleResDto?> CreateAsync(RoleReqDto role);

        /// <summary>
        /// Updates an existing role by id. SystemAdmin (id=1) cannot be updated.
        /// </summary>
        Task<RoleResDto?> UpdateAsync(int id, RoleReqDto role);

        /// <summary>
        /// Deletes a role by id. SystemAdmin (id=1) cannot be deleted.
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Sets the active status of a role. SystemAdmin (id=1) cannot be modified.
        /// </summary>
        Task<RoleResDto?> SetActiveStatusAsync(int id, bool isActive);
    }
}