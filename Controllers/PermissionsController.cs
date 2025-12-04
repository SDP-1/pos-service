using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.Enums;
using pos_service.Services;
using pos_service.Services.Permissions;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.SystemAdmin)]
    public class PermissionsController : SystemBaseController
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _permissionService.GetAllPermissionsAsync());

        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetForRole(UserRole role) => Ok(await _permissionService.GetPermissionsForRoleAsync(role));

        [HttpPost("role/{role}/add/{permission}")]
        public async Task<IActionResult> AddPermission(UserRole role, string permission)
        {
            var success = await _permissionService.AddPermissionToRoleAsync(role, permission);
            return success ? Ok() : Conflict("Permission already exists for role");
        }

        [HttpDelete("role/{role}/remove/{permission}")]
        public async Task<IActionResult> RemovePermission(UserRole role, string permission)
        {
            var success = await _permissionService.RemovePermissionFromRoleAsync(role, permission);
            return success ? Ok() : NotFound();
        }
    }
}
