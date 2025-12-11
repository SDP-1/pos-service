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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            EnsurePermission(PermissionType.ROLE_VIEW);
            var roles = await _roleService.GetAllAsync();

            // hide SystemAdmin from users who do not have PERMISSION_SYSADMIN_VIEW
            var canSeeSysAdmin =_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW);
            if (!canSeeSysAdmin)
            {
                roles = roles.Where(r => r.Id != 1);
            }

            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        [Permission(PermissionType.ROLE_VIEW)]
        public async Task<IActionResult> Get(int id)
        {
            // Only users with PERMISSION_SYSADMIN_VIEW (or the system admin themselves) may view the SystemAdmin role
            if (id == 1 && !(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                throw new PermissionDeniedException("Insufficient permission to view SystemAdmin role");

            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        [Permission(PermissionType.ROLE_CREATE)]
        public async Task<IActionResult> Create(RoleReqDto role)
        {
            // Prevent creating or tampering with SystemAdmin unless caller has PERMISSION_SYSADMIN_VIEW
            if (string.Equals(role.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase) || role.Id == 1)
            {
                if (!(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                    throw new PermissionDeniedException("Insufficient permission to create or modify SystemAdmin role");
            }

            var created = await _roleService.CreateAsync(role);
            if (created == null) return Conflict("Role exists or invalid");
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Permission(PermissionType.ROLE_UPDATE)]
        public async Task<IActionResult> Update(int id, RoleReqDto role)
        {
            // Prevent updates to SystemAdmin unless caller has PERMISSION_SYSADMIN_VIEW or is the SystemAdmin
            if (id == 1 || string.Equals(role.Name, "SystemAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                    throw new PermissionDeniedException("Insufficient permission to update SystemAdmin role");
            }

            var updated = await _roleService.UpdateAsync(id, role);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Permission(PermissionType.ROLE_DELETE)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 1 && !(_currentUser.HasPermission(PermissionType.PERMISSION_SYSADMIN_VIEW)))
                throw new PermissionDeniedException("Insufficient permission to delete SystemAdmin role");

            var deleted = await _roleService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}