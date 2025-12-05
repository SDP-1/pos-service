using pos_service.Models;
using pos_service.Services.Permissions;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor        _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;
        private readonly IPermissionService          _permissionService;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUserService> logger,
            IPermissionService permissionService)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger              = logger;
            _permissionService   = permissionService;
        }

        /// <summary>
        /// Retrieves the current authenticated user's information.
        /// This method does not cache results across requests to avoid leaking data between requests.
        /// </summary>
        /// <returns>The current user details including identity and role information.</returns>
        public CurrentUser GetCurrentUser()
        {
            try
            {
                if (_httpContextAccessor?.HttpContext == null)
                {
                    _logger.LogDebug("HttpContext or HttpContextAccessor is null");
                    return new CurrentUser { IsAuthenticated = false };
                }

                var principal = _httpContextAccessor.HttpContext.User;

                if (principal?.Identity?.IsAuthenticated != true)
                {
                    return new CurrentUser { IsAuthenticated = false };
                }

                var currentUser = CurrentUser.FromClaimsPrincipal(principal);

                // populate permissions from role service
                try
                {
                    var perms = _permissionService.GetPermissionsForRoleAsync(currentUser.RoleId).GetAwaiter().GetResult();
                    var permNames = perms.Select(p => p.Name).ToList();

                    // store in the CurrentUser.Permissions collection for direct use
                    currentUser.Permissions = permNames;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load permissions for role {RoleId}", currentUser.RoleId);
                    currentUser.Permissions = new List<string>();
                }

                _logger.LogDebug("Current user retrieved: UserId={UserId}, Uuid={Uuid}, UserName={UserName}",
                    currentUser.Id, currentUser.Uuid, currentUser.UserName);

                return currentUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return new CurrentUser { IsAuthenticated = false };
            }
        }

        // Quick access methods

        /// <summary>
        /// Checks if the current user is in any of the specified roles.
        /// </summary>
        public bool IsInRole(params int[] roles) => GetCurrentUser().IsInRole(roles);

        /// <summary>
        /// Checks if the current user has the specified permission.
        /// </summary>
        public bool HasPermission(PermissionType permission) => GetCurrentUser().HasPermission(permission);

        /// <summary>
        /// Checks if the current user has permission to manage other users.
        /// </summary>
        public bool CanManageUsers() => GetCurrentUser().CanManageUsers();

        /// <summary>
        /// Checks if the current user has permission to view sensitive data.
        /// </summary>
        public bool CanViewSensitiveData() => GetCurrentUser().CanViewSensitiveData();

        // Validation methods

        /// <summary>
        /// Ensures that the current user is authenticated.
        /// Throws an exception if the user is not authenticated.
        /// </summary>
        public void EnsureAuthenticated()
        {
            if (!GetCurrentUser().IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");
        }

        /// <summary>
        /// Ensures that the current user has any of the specified roles.
        /// Throws an exception if the user does not have the required roles.
        /// </summary>
        public void EnsureRole(params int[] roles)
        {
            EnsureAuthenticated();
            if (!IsInRole(roles))
                throw new UnauthorizedAccessException($"User does not have required role. Required: {string.Join(", ", roles)}");
        }

        /// <summary>
        /// Ensures that the current user has the specified permission.
        /// Throws an exception if the user does not have the required permission.
        /// </summary>
        public void EnsurePermission(PermissionType permission)
        {
            EnsureAuthenticated();
            if (!HasPermission(permission))
                throw new UnauthorizedAccessException($"User does not have required permission: {permission}");
        }
    }
}
