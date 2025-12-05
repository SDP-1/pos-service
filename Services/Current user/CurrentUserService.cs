using pos_service.Models;
using pos_service.Services.Permissions;

namespace pos_service.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor        _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;
        private readonly IPermissionService          _permissionService;
        private CurrentUser                          _currentUser;
        private bool                                 _initialized = false;
        private readonly object                      _lock = new object();

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
        /// </summary>
        /// <returns>The current user details including identity and role information.</returns>
        public CurrentUser GetCurrentUser()
        {
            if (_initialized)
                return _currentUser;

            lock (_lock)
            {
                if (_initialized)
                    return _currentUser;

                try
                {
                    if (_httpContextAccessor?.HttpContext == null)
                    {
                        _logger.LogDebug("HttpContext or HttpContextAccessor is null");
                        _currentUser = new CurrentUser { IsAuthenticated = false };
                        _initialized = true;
                        return _currentUser;
                    }

                    var principal = _httpContextAccessor.HttpContext.User;

                    if (principal?.Identity?.IsAuthenticated != true)
                    {
                        _currentUser = new CurrentUser { IsAuthenticated = false };
                        _initialized = true;
                        return _currentUser;
                    }

                    _currentUser = CurrentUser.FromClaimsPrincipal(principal);

                    // populate permissions
                    try
                    {
                        var perms = _permissionService.GetPermissionsForRoleAsync(_currentUser.RoleId).GetAwaiter().GetResult();
                        // store as simple names in claims-like list
                        _currentUserPermissions = perms.Select(p => p.Name).ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load permissions for role {RoleId}", _currentUser.RoleId);
                        _currentUserPermissions = new List<string>();
                    }

                    _logger.LogDebug("Current user retrieved: UserId={UserId}, Uuid={Uuid}, UserName={UserName}",
                        _currentUser.Id, _currentUser.Uuid, _currentUser.UserName);

                    _initialized = true;
                    return _currentUser;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving current user");
                    _currentUser = new CurrentUser { IsAuthenticated = false };
                    _initialized = true;
                    return _currentUser;
                }
            }
        }

        // hold permissions locally
        private List<string> _currentUserPermissions = new List<string>();

        // Quick access methods

        /// <summary>
        /// Checks if the current user is in any of the specified roles.
        /// </summary>
        public bool IsInRole(params int[] roles) => GetCurrentUser().IsInRole(roles);

        /// <summary>
        /// Checks if the current user has the specified permission.
        /// </summary>
        public bool HasPermission(string permission) => GetCurrentUser().IsAuthenticated && _currentUserPermissions.Contains(permission);

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
        public void EnsurePermission(string permission)
        {
            EnsureAuthenticated();
            if (!HasPermission(permission))
                throw new UnauthorizedAccessException($"User does not have required permission: {permission}");
        }
    }
}
