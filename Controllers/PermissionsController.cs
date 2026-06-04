using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Services.Permissions;
using pos_service.Services;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [Authorize] // Only authenticated users; permission checks are done in service or controller
    [Authorize]
    public class PermissionsController : SystemBaseController
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Retrieves all permission definitions available in the system.
        /// </summary>
        /// <returns>200 OK with the list of permissions.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _permissionService.GetAllPermissionsAsync());

        /// <summary>
        /// Retrieves permissions assigned to a specific role.
        /// </summary>
        /// <param name="roleId">Identifier of the role to query permissions for.</param>
        /// <returns>200 OK with the list of permissions for the role.</returns>
        [HttpGet("role/{roleId:int}")]
        public async Task<IActionResult> GetForRole(int roleId) => Ok(await _permissionService.GetPermissionsForRoleAsync(roleId));

        /// <summary>
        /// Adds a permission to a role.
        /// </summary>
        /// <param name="roleId">Role identifier.</param>
        /// <param name="permission">Permission key/name to add.</param>
        /// <returns>200 OK on success or 409 Conflict if already exists.</returns>
        [HttpPost("role/{roleId:int}/add/{permission}")]
        public async Task<IActionResult> AddPermission(int roleId, string permission)
        {
            var success = await _permissionService.AddPermissionToRoleAsync(roleId, permission);
            return success ? Ok() : Conflict("Permission already exists for role");
        }

        /// <summary>
        /// Removes a permission from a role.
        /// </summary>
        /// <param name="roleId">Role identifier.</param>
        /// <param name="permission">Permission key/name to remove.</param>
        /// <returns>200 OK on success or 404 NotFound if the permission wasn't found for the role.</returns>
        [HttpDelete("role/{roleId:int}/remove/{permission}")]
        public async Task<IActionResult> RemovePermission(int roleId, string permission)
        {
            var success = await _permissionService.RemovePermissionFromRoleAsync(roleId, permission);
            return success ? Ok() : NotFound();
        }
    }
}
