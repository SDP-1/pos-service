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

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _permissionService.GetAllPermissionsAsync());

        [HttpGet("role/{roleId:int}")]
        public async Task<IActionResult> GetForRole(int roleId) => Ok(await _permissionService.GetPermissionsForRoleAsync(roleId));

        [HttpPost("role/{roleId:int}/add/{permission}")]
        public async Task<IActionResult> AddPermission(int roleId, string permission)
        {
            var success = await _permissionService.AddPermissionToRoleAsync(roleId, permission);
            return success ? Ok() : Conflict("Permission already exists for role");
        }

        [HttpDelete("role/{roleId:int}/remove/{permission}")]
        public async Task<IActionResult> RemovePermission(int roleId, string permission)
        {
            var success = await _permissionService.RemovePermissionFromRoleAsync(roleId, permission);
            return success ? Ok() : NotFound();
        }
    }
}
