using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pos_service.Models.DTO.User;
using pos_service.Models.Enums;
using pos_service.Services;
using System.Security.Claims;

namespace pos_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // --- AUTHENTICATION ENDPOINTS (Public/Unsecured) ---

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        // POST: api/users/login
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


        // --- USER MANAGEMENT ENDPOINTS (Secured - Requires SystemAdmin/Manager Role) ---

        /// <summary>
        /// Retrieves a list of all users. (SystemAdmin access required)
        /// </summary>
        // GET: api/users
        [HttpGet]
        [Authorize(Roles = UserRoles.AllAdmins)]
        public async Task<ActionResult<IEnumerable<UserResDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by ID.
        /// </summary>
        // GET: api/users/5
        [HttpGet("{id:int}")]
        [Authorize(Roles = UserRoles.DayToDayOperations)] // Allow Cashier to view their own profile (to be implemented in service)
        public async Task<ActionResult<UserResDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Creates a new user. (SystemAdmin access required)
        /// </summary>
        // POST: api/users
        [HttpPost]
        [Authorize(Roles = UserRoles.AllAdmins)]
        public async Task<ActionResult<UserResDto>> CreateUser([FromBody] UserReqDto userDto)
        {
            var newUser = await _userService.CreateUserAsync(userDto);
            if (newUser == null)
            {
                return Conflict("A user with this username already exists.");
            }
            // Use the User's Id as the route parameter
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        /// <summary>
        /// Updates an existing user's details. (SystemAdmin/Self access required)
        /// </summary>
        // PUT: api/users/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.AllAdmins)] // Allow managers/SystemAdmins to update users
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserReqDto userDto)
        {
            var success = await _userService.UpdateUserAsync(id, userDto);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Deactivates a user account (soft delete). (SystemAdmin access required)
        /// </summary>
        // PATCH: api/users/5/deactivate
        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = UserRoles.AllAdmins)]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var success = await _userService.DeactivateUserAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Permanently deletes a user. (High-level SystemAdmin access required)
        /// </summary>
        // DELETE: api/users/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.AllAdmins)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // --- PROFILE/SECURITY ENDPOINTS ---

        /// <summary>
        /// Allows a user to change their own password.
        /// </summary>
        // PATCH: api/users/5/change-password
        [HttpPatch("{id:int}/change-password")]
        [Authorize] // Any logged-in user
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto passwordDto)
        {
            // Assuming ChangePasswordDto contains OldPassword and NewPassword
            var success = await _userService.ChangePasswordAsync(id, passwordDto.OldPassword, passwordDto.NewPassword);
            if (!success)
            {
                return BadRequest("Invalid ID or incorrect old password.");
            }
            return NoContent();
        }
    }
}
