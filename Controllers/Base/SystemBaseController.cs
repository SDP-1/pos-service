using Microsoft.AspNetCore.Mvc;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Services;

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

        protected void EnsureRole(params UserRole[] roles)
        {
            EnsureAuthenticated();
            if (!_currentUser.IsInRole(roles))
                throw new UnauthorizedAccessException($"User does not have required role. Required: {string.Join(", ", roles)}");
        }

        protected void EnsurePermission(string permission)
        {
            EnsureAuthenticated();
            if (!_currentUser.HasPermission(permission))
                throw new UnauthorizedAccessException($"User does not have required permission: {permission}");
        }

        // Quick access properties for convenience
        protected int CurrentUserId         => _currentUser.Id;
        protected string CurrentUserUuid    => _currentUser.Uuid;
        protected string CurrentUserName    => _currentUser.UserName;
        protected UserRole CurrentUserRole  => _currentUser.Role;
        protected bool IsUserAuthenticated  => _currentUser.IsAuthenticated;

        // Common authorization checks
        protected bool CanManageUsers       => _currentUser.CanManageUsers();
        protected bool CanViewSensitiveData => _currentUser.CanViewSensitiveData();
    }
}
