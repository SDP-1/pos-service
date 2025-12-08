using Microsoft.AspNetCore.Mvc;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Services;
using pos_service.Exceptions;

namespace pos_service.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemBaseController : ControllerBase
    {
        protected readonly CurrentUser       _currentUser;
        private readonly ICurrentUserService _currentUserService;

        public SystemBaseController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            _currentUser        = _currentUserService.GetCurrentUser();
        }

        // Helper methods for common authorization checks
        protected void EnsureAuthenticated()
        {
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");
        }

        protected void EnsureRole(params int[] roles)
        {
            EnsureAuthenticated();
            if (!_currentUser.IsInRole(roles))
                throw new UnauthorizedAccessException($"User does not have required role. Required: {string.Join(", ", roles)}");
        }

        protected void EnsurePermission(PermissionType permission)
        {
            EnsureAuthenticated();
            if (!_currentUser.HasPermission(permission))
                throw new PermissionDeniedException($"User does not have required permission: {permission}");
        }
    }
}
