using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Users;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Security;
using pos_service.Services.Common.Cache;
using pos_service.Helpers;

using pos_service.Data;

namespace pos_service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IContactService _contactService;
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtGeneratorService _jwtGenerator;
        private readonly ICacheService _cacheService;

        public UserService(
            IUserRepository repo, 
            IContactService contactService,
            IContactRepository contactRepository,
            IMapper mapper, 
            IPasswordHasherService hasher, 
            IJwtGeneratorService jwt, 
            ICacheService cacheService)
        {
            _userRepository     = repo;
            _contactService     = contactService;
            _contactRepository  = contactRepository;
            _mapper             = mapper;
            _passwordHasher     = hasher;
            _jwtGenerator       = jwt;
            _cacheService       = cacheService;
        }

        /// <summary>
        /// Retrieves all users from the system.
        /// </summary>
        /// <param name="currentUser">The current user requesting the user list.</param>
        /// <returns>A list of all user details.</returns>
        public async Task<IEnumerable<UserResDto>> GetAllUsersAsync(CurrentUser currentUser)
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResDto>>(users);
        }

        /// <summary>
        /// Retrieves a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="currentUser">The current user requesting the user details.</param>
        /// <returns>The user details if found, otherwise null.</returns>
        public async Task<UserResDto?> GetUserByIdAsync(int id, CurrentUser currentUser)
        {
            var user = await _userRepository.GetByIdWithContactsAsync(id);
            return _mapper.Map<UserResDto?>(user);
        }

        public async Task<UserResDto?> CreateUserAsync(UserReqDto userDto, CurrentUser currentUser)
        {
            // 1. Check for existing user by UserName
            if (await _userRepository.GetByUserNameAsync(userDto.UserName) != null)
            {
                return null; // Conflict: User already exists
            }

            // 2. Map DTO to Entity and Hash Password
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);

            // 3. Convert profile image to bytes if provided
            user.ProfileImage = await FileHelper.ConvertFileToBytesAsync(userDto.ProfileImage);

            List<Contact>? contacts = null;
            if (userDto.Contacts != null && userDto.Contacts.Any())
            {
                contacts = userDto.Contacts.Select(contactDto =>
                {
                    var contact      = _mapper.Map<Contact>(contactDto);
                    contact.Uuid     = Guid.NewGuid().ToString();
                    contact.IsActive = contactDto.IsActive;
                    return contact;
                }).ToList();
            }

            var newUser = await _userRepository.SaveNewUserWithContactsAsync(user, contacts);

            return _mapper.Map<UserResDto>(newUser);
        }

        public async Task<bool> DeactivateUserAsync(int id, CurrentUser currentUser)
        {
            var userToUpdate = await _userRepository.GetByIdAsync(id);
            if (userToUpdate == null) return false;

            userToUpdate.IsActive = false;

            //clear all system cache wen user delete
            _cacheService.ClearAll();

            await _userRepository.UpdateAsync(userToUpdate);
            return true;
        }

        /// <summary>
        /// Sets a user account's IsActive status to true.
        /// </summary>
        public async Task<bool> ActivateUserAsync(int id, CurrentUser currentUser)
        {
            var userToUpdate = await _userRepository.GetByIdAsync(id);
            if (userToUpdate == null)
            {
                return false; // User not found
            }

            if (userToUpdate.IsActive)
            {
                return true; // Already active, no change needed
            }

            userToUpdate.IsActive = true;

            await _userRepository.UpdateAsync(userToUpdate);
            return true;
        }

        public async Task<(UserResDto? User, string? Token)> LoginAsync(UserLoginReqDto loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.UserName);

            if (user == null || !user.IsActive)
            {
                return (null, null); // User not found or inactive
            }

            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return (null, null); // Invalid password
            }

            // Authentication successful, generate JWT
            var token = _jwtGenerator.GenerateToken(user);

            try
            {
                // Cache the basic user for token validation and current user lookup
                _cacheService.Set(ServiceCacheKey.Users, user.Uuid, user);
            }
            catch { /* ignore cache errors */ }

            return (_mapper.Map<UserResDto>(user), token);
        }

        /// <summary>
        /// Invalidates any cached entries related to a user (logout).
        /// </summary>
        public Task LogoutAsync(string uuid)
        {
            if (string.IsNullOrEmpty(uuid)) return Task.CompletedTask;
            try
            {
                _cacheService.Remove(ServiceCacheKey.Users, uuid);
                _cacheService.Remove(ServiceCacheKey.Permissions, uuid);
            }
            catch { /* ignore cache errors */ }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Changes a user's password after verifying the old password.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int id, string oldPassword, string newPassword, CurrentUser currentUser)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false; // User not found
            }

            // 1. Verify the old password
            if (!_passwordHasher.VerifyPassword(oldPassword, user.PasswordHash))
            {
                return false; // Old password does not match
            }

            // 2. Hash and update the new password
            user.PasswordHash = _passwordHasher.HashPassword(newPassword);

            // Update Auditable property
            user.UpdatedBy = user.UserName; // Assuming the user changes their own password

            await _userRepository.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// Resets a user's password without needing the current password.
        /// </summary>
        public async Task<bool> ResetPasswordAsync(int id, string newPassword, CurrentUser currentUser)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false; // User not found
            }

            // 1. Hash and update the new password
            user.PasswordHash = _passwordHasher.HashPassword(newPassword);

            // Update Auditable property
            user.UpdatedBy = currentUser.UserName;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// Updates an existing user's details, including mapping contacts and handling the password/profile image.
        /// </summary>
        public async Task<UserResDto?> UpdateUserAsync(int id, UserReqDto userDto, CurrentUser currentUser)
        {
            // Fetch the user with related contacts for comprehensive update
            var userToUpdate = await _userRepository.GetByIdWithContactsAsync(id);
            if (userToUpdate == null)
            {
                return null; // User not found
            }

            // 1. Check if the new username is already taken by another user
            if (!string.IsNullOrEmpty(userDto.UserName)) 
            {
                var userByUserName = await _userRepository.GetByUserNameAsync(userDto.UserName);
                if (userByUserName != null && userByUserName.Id != id)
                {
                    // Conflict: Another user already has this username (email)
                    return null;
                }
            }

            // 2. Handle profile image according to DTO flags:
            if (userDto.RemoveImage)
                userToUpdate.ProfileImage = null;
            else if (userDto.ProfileImage != null)
                userToUpdate.ProfileImage = await FileHelper.ConvertFileToBytesAsync(userDto.ProfileImage);

            // 3. Map incoming DTO properties into existing entity
            if (userDto.FirstName != null      ) userToUpdate.FirstName         = userDto.FirstName;
            if (userDto.LastName != null       ) userToUpdate.LastName          = userDto.LastName;
            if (userDto.RoleId != null         ) userToUpdate.RoleId            = userDto.RoleId.Value;
            if (userDto.UserName != null       ) userToUpdate.UserName          = userDto.UserName;
            userToUpdate.NIC = userDto.NIC;

            // 4. Handle password change
            if (!string.IsNullOrEmpty(userDto.Password))
                userToUpdate.PasswordHash = _passwordHasher.HashPassword(userDto.Password);

            var data = await _userRepository.UpdateAsync(userToUpdate);

            // 5. Merge contacts: update existing, add new, delete removed
            if (userDto.Contacts != null)
                await _contactService.MergeContactsAsync(ContactOwnerType.User, id, userDto.Contacts);

            return _mapper.Map<UserResDto>(data);
        }

        /// <summary>
        /// Permanently deletes a user from the database by ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>True if the user was successfully deleted, false if the user was not found.</returns>
        public async Task<bool> DeleteUserAsync(int id, CurrentUser currentUser)
        {
            var userToDelete = await _userRepository.GetByIdAsync(id);
            if (userToDelete == null)
            {
                // User not found
                return false;
            }

            //clear all system cache wen user delete
            _cacheService.ClearAll();

            // Perform the permanent deletion
            await _userRepository.DeleteAsync(userToDelete);
            return true;
        }
    }
}
