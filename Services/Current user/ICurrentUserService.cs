using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Retrieves the current authenticated user's information.
        /// </summary>
        /// <returns>The current user details including identity and role information.</returns>
        CurrentUser GetCurrentUser();

        /// <summary>
        /// Checks if the current user is in any of the specified roles.
        /// </summary>
        /// <param name="roles">The roles to check against the current user's role ids.</param>
        /// <returns>True if the current user has any of the specified roles, otherwise false.</returns>
        bool IsInRole(params int[] roles);

        /// <summary>
        /// Checks if the current user has the specified permission.
        /// </summary>
        /// <param name="permission">The permission to check.</param>
        /// <returns>True if the current user has the specified permission, otherwise false.</returns>
        bool HasPermission(PermissionType permission);

        /// <summary>
        /// Checks if the current user has permission to manage other users.
        /// </summary>
        /// <returns>True if the current user can manage users, otherwise false.</returns>
        bool CanManageUsers();

        /// <summary>
        /// Checks if the current user has permission to view sensitive data.
        /// </summary>
        /// <returns>True if the current user can view sensitive data, otherwise false.</returns>
        bool CanViewSensitiveData();

        /// <summary>
        /// Ensures that the current user is authenticated.
        /// Throws an exception if the user is not authenticated.
        /// </summary>
        void EnsureAuthenticated();

        /// <summary>
        /// Ensures that the current user has any of the specified roles.
        /// Throws an exception if the user does not have the required roles.
        /// </summary>
        /// <param name="roles">The required role ids.</param>
        void EnsureRole(params int[] roles);

        /// <summary>
        /// Ensures that the current user has the specified permission.
        /// Throws an exception if the user does not have the required permission.
        /// </summary>
        /// <param name="permission">The required permission.</param>
        void EnsurePermission(PermissionType permission);
    }
}