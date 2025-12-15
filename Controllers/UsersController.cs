using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Controllers.Base;
using pos_service.Models.DTO.Users;
using pos_service.Services;

namespace pos_service.Controllers
{
    /// <summary>
    /// Controller for managing user authentication, profiles, and user management in the POS system.
    /// Provides endpoints for login, user CRUD operations, and password management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : SystemBaseController
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// Initializes a new instance of the UsersController class.
        /// </summary>
        /// <param name="userService">The user service for authentication and user management operations.</param>
        public UsersController(IUserService userService, ICurrentUserService currentUserService) : base(currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginDto">The user login credentials containing username and password.</param>
        /// <returns>User details and JWT token if authentication successful, otherwise returns Unauthorized.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginReqDto loginDto)
        {
            var result = await _userService.LoginAsync(loginDto);
            if (result.User == null || result.Token == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            // Typically return an object containing the User details and the Token
            return Ok(new { User = result.User, Token = result.Token });
        }

        /// <summary>
        /// Logs out the current user by invalidating their cached data.
        /// Note: JWTs are stateless so this only clears server-side cache entries.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync(_currentUser.Uuid);
            return Ok("Logged out");
        }

        /// <summary>
        /// Retrieves a list of all users in the system.
        /// </summary>
        /// <returns>A list of all user details.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync(_currentUser);
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user details if found, otherwise returns NotFound.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id, _currentUser);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="userDto">The user data transfer object containing user information.</param>
        /// <returns>The newly created user details with location header.</returns>
        [HttpPost]
        public async Task<ActionResult<UserResDto>> CreateUser([FromBody] UserReqDto userDto)
        {
            try
            {
                var newUser = await _userService.CreateUserAsync(userDto, _currentUser);
                if (newUser == null)
                {
                    return Conflict("A user with this username already exists.");
                }

                // Use the User's Id as the route parameter
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
            catch (FileNotFoundException ex)
            {
                // Missing source file provided by client
                return BadRequest($"Profile image not found: {ex.FileName}");
            }
            catch (Exception ex)
            {
                // Generic server error when saving/copying images
                return StatusCode(500, $"Error saving profile image: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing user's details.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="userDto">The user data transfer object containing updated information.</param>
        /// <returns>The updated user details if successful, otherwise returns NotFound.</returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserReqDto userDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, userDto, _currentUser);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (FileNotFoundException ex)
            {
                return BadRequest($"Profile image not found: {ex.FileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving profile image: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivates a user account (soft delete).
        /// </summary>
        /// <param name="id">The unique identifier of the user to deactivate.</param>
        /// <returns>NoContent if successful, otherwise returns NotFound.</returns>
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var success = await _userService.DeactivateUserAsync(id, _currentUser);
            if (!success)
            {
                return NotFound();
            }
            return Ok("User deactivation successfull.");
        }

        /// <summary>
        /// Permanently deletes a user from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>NoContent if successful, otherwise returns NotFound.</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id, _currentUser);
            if (!success)
            {
                return NotFound();
            }
            return Ok("User Delete Successfull.");
        }

        /// <summary>
        /// Allows a user to change their own password.
        /// </summary>
        /// <param name="id">The unique identifier of the user changing their password.</param>
        /// <param name="passwordDto">The password change data containing old and new passwords.</param>
        /// <returns>NoContent if successful, otherwise returns BadRequest.</returns>
        [HttpPatch("{id:int}/change-password")]
        [Authorize] // Any logged-in user
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto passwordDto)
        {
            // Assuming ChangePasswordDto contains OldPassword and NewPassword
            var success = await _userService.ChangePasswordAsync(id, passwordDto.OldPassword, passwordDto.NewPassword, _currentUser);
            if (!success)
            {
                return BadRequest("Invalid ID or incorrect old password.");
            }
            return Ok("Change passwrd succesfull");
        }
    }
}