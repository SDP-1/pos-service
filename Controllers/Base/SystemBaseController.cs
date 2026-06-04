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
        /// <summary>
        /// Ensures the current request is authenticated. Throws UnauthorizedAccessException when not.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when the current user is not authenticated.</exception>
        protected void EnsureAuthenticated()
        {
            // Ensure the current user has been authenticated in the request context
            if (!_currentUser.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");
        }

        /// <summary>
        /// Ensures the current user belongs to at least one of the specified role ids.
        /// </summary>
        /// <param name="roles">Array of role ids that are permitted.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not in any of the provided roles.</exception>
        protected void EnsureRole(params int[] roles)
        {
            // Verify user is authenticated and belongs to one of the required roles
            EnsureAuthenticated();
            if (!_currentUser.IsInRole(roles))
                throw new UnauthorizedAccessException($"User does not have required role. Required: {string.Join(", ", roles)}");
        }

        /// <summary>
        /// Ensures the current user has the specified permission.
        /// </summary>
        /// <param name="permission">Permission required for the current operation.</param>
        /// <exception cref="PermissionDeniedException">Thrown when the user lacks the permission.</exception>
        protected void EnsurePermission(PermissionType permission)
        {
            // Ensure user is authenticated and has a specific permission
            EnsureAuthenticated();
            if (!_currentUser.HasPermission(permission))
                throw new PermissionDeniedException($"User does not have required permission: {permission}");
        }
    }
}
