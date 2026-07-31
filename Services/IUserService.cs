using pos_service.Models;
using pos_service.Models.DTO.Users;

namespace pos_service.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the user list.</param>
        /// <returns>A list of all user details.</returns>
        Task<IEnumerable<UserResDto>> GetAllUsersAsync(CurrentUser currentUser);

        /// <summary>
        /// Retrieves a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="currentUser">The current user requesting the user details.</param>
        /// <returns>The user details if found, otherwise null.</returns>
        Task<UserResDto?> GetUserByIdAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="userDto">The user data transfer object containing user information.</param>
        /// <param name="currentUser">The current user creating the new user.</param>
        /// <returns>The newly created user details if successful, otherwise null.</returns>
        Task<UserResDto?> CreateUserAsync(UserReqDto userDto, CurrentUser currentUser);

        /// <summary>
        /// Updates an existing user's details.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="userDto">The user data transfer object containing updated information.</param>
        /// <param name="currentUser">The current user performing the update.</param>
        /// <returns>The updated user details if successful, otherwise null.</returns>
        Task<UserResDto?> UpdateUserAsync(int id, UserReqDto userDto, CurrentUser currentUser);

        /// <summary>
        /// Permanently deletes a user from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <param name="currentUser">The current user performing the deletion.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteUserAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Deactivates a user account (soft delete).
        /// </summary>
        /// <param name="id">The unique identifier of the user to deactivate.</param>
        /// <param name="currentUser">The current user performing the deactivation.</param>
        /// <returns>True if deactivation was successful, otherwise false.</returns>
        Task<bool> DeactivateUserAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Activates a previously deactivated user account.
        /// </summary>
        /// <param name="id">The unique identifier of the user to activate.</param>
        /// <param name="currentUser">The current user performing the activation.</param>
        /// <returns>True if activation was successful, otherwise false.</returns>
        Task<bool> ActivateUserAsync(int id, CurrentUser currentUser);

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="loginDto">The user login credentials containing username and password.</param>
        /// <returns>A tuple containing user details and JWT token if authentication successful, otherwise null values.</returns>
        Task<(UserResDto? User, string? Token)> LoginAsync(UserLoginReqDto loginDto);

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        /// <param name="id">The unique identifier of the user changing their password.</param>
        /// <param name="oldPassword">The user's current password.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <param name="currentUser">The current user changing the password.</param>
        /// <returns>True if password change was successful, otherwise false.</returns>
        Task<bool> ChangePasswordAsync(int id, string oldPassword, string newPassword, CurrentUser currentUser);

        /// <summary>
        /// Resets a user's password without needing the current password.
        /// </summary>
        /// <param name="id">The unique identifier of the user whose password is being reset.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <param name="currentUser">The current user performing the reset.</param>
        /// <returns>True if password reset was successful, otherwise false.</returns>
        Task<bool> ResetPasswordAsync(int id, string newPassword, CurrentUser currentUser);

        /// <summary>
        /// Clears any server-side cached entries for a user (logout).
        /// JWTs are stateless; this only removes cached user/permission data.
        /// </summary>
        Task LogoutAsync(string uuid);
    }
}