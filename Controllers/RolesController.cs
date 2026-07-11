using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Services.Roles;
using pos_service.Services;
using pos_service.Exceptions;
using pos_service.Authorization;
using pos_service.Models.DTO.Roles;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : SystemBaseController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Retrieves all roles, hiding SystemAdmin for users without the special view permission.
        /// </summary>
        /// <returns>200 OK with the list of roles.</returns>
        [HttpGet]
        [Permission(PermissionType.ROLE_VIEW)]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();

            // hide SystemAdmin from users who do not have PERMISSION_SYSADMIN_VIEW
            var canSeeSysAdmin = _currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW);
            if (!canSeeSysAdmin)
            {
                roles = roles.Where(r => r.Id != (int)UserRole.SYSTEM_ADMIN);
            }

            return Ok(roles);
        }

        /// <summary>
        /// Retrieves all active roles, with SystemAdmin hidden for most users.
        /// </summary>
        /// <returns>200 OK with the list of active roles.</returns>
        [HttpGet("active")]
        [Permission(PermissionType.ROLE_VIEW)]
        public async Task<IActionResult> GetActiveRoles()
        {
            var roles = await _roleService.GetActiveAsync();

            // hide SystemAdmin from users who do not have PERMISSION_SYSADMIN_VIEW
            var canSeeSysAdmin = _currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW);
            if (!canSeeSysAdmin)
            {
                roles = roles.Where(r => r.Id != (int)UserRole.SYSTEM_ADMIN);
            }

            return Ok(roles);
        }

        /// <summary>
        /// Retrieves a role by id. Access to the SystemAdmin role is restricted.
        /// </summary>
        /// <param name="id">Role identifier.</param>
        /// <returns>200 OK with the role when found; 404 NotFound otherwise.</returns>
        [HttpGet("{id:int}")]
        [Permission(PermissionType.ROLE_VIEW)]
        public async Task<IActionResult> Get(int id)
        {
            // Only users with PERMISSION_SYSADMIN_VIEW (or the system admin themselves) may view the SystemAdmin role
            if (id == (int)UserRole.SYSTEM_ADMIN && !(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                throw new PermissionDeniedException("Insufficient permission to view SystemAdmin role");

            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        /// <summary>
        /// Creates a new role. Creation of SystemAdmin role requires elevated permission.
        /// </summary>
        /// <param name="role">Role creation DTO.</param>
        /// <returns>201 Created with the new role or 409 Conflict when role exists/invalid.</returns>
        [HttpPost]
        [Permission(PermissionType.ROLE_CREATE)]
        public async Task<IActionResult> Create(RoleReqDto role)
        {
            // Prevent creating or tampering with SystemAdmin unless caller has PERMISSION_SYSADMIN_VIEW
            if (string.Equals(role.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase) || role.Id == (int)UserRole.SYSTEM_ADMIN)
            {
                if (!(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                    throw new PermissionDeniedException("Insufficient permission to create or modify SystemAdmin role");
            }

            var created = await this._roleService.CreateAsync(role);
            if (created == null) return Conflict("Role exists or invalid");
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing role. Modifying SystemAdmin requires elevated permission.
        /// </summary>
        /// <param name="id">Role identifier to update.</param>
        /// <param name="role">Role update DTO.</param>
        /// <returns>200 OK with updated role or 404 NotFound.</returns>
        [HttpPut("{id:int}")]
        [Permission(PermissionType.ROLE_UPDATE)]
        public async Task<IActionResult> Update(int id, RoleReqDto role)
        {
            // Prevent updates to SystemAdmin unless caller has PERMISSION_SYSADMIN_VIEW or is the SystemAdmin
            if (id == (int)UserRole.SYSTEM_ADMIN || string.Equals(role.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                    throw new PermissionDeniedException("Insufficient permission to update SystemAdmin role");
            }

            var updated = await this._roleService.UpdateAsync(id, role);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a role. SystemAdmin cannot be deleted.
        /// </summary>
        /// <param name="id">Role identifier to delete.</param>
        /// <returns>200 OK when deleted or 404 NotFound.</returns>
        [HttpDelete("{id:int}")]
        [Permission(PermissionType.ROLE_DELETE)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == (int)UserRole.SYSTEM_ADMIN)
                throw new InvalidOperationException("SystemAdmin role cannot be deleted.");

            var deleted = await this._roleService.DeleteAsync(id);
            if (!deleted) return NotFound();

            return Ok("Role Delete Successful.");
        }

        /// <summary>
        /// Sets the active status of a role (enable/disable).
        /// </summary>
        /// <param name="id">Role identifier.</param>
        /// <param name="isActive">True to activate; false to deactivate.</param>
        /// <returns>200 OK with updated role or 404 NotFound.</returns>
        [HttpPut("{id:int}/status")]
        [Permission(PermissionType.ROLE_UPDATE)]
        public async Task<IActionResult> SetActiveStatus(int id, [FromBody] bool isActive)
        {
            if (id == (int)UserRole.SYSTEM_ADMIN && !(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                throw new PermissionDeniedException("Insufficient permission to modify SystemAdmin role");

            var updated = await this._roleService.SetActiveStatusAsync(id, isActive);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
    }
}