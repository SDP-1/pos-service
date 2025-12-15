using AutoMapper;
using pos_service.Models;
using pos_service.Models.DTO.Users;
using pos_service.Repositories;
using pos_service.Security;
using pos_service.Services.Common;
using pos_service.Services.Common.Cache;

namespace pos_service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICacheService _cacheService;

        public UserService(IUserRepository repo, IMapper mapper, IPasswordHasher hasher, IJwtGenerator jwt, IFileStorageService fileStorageService, Services.Common.Cache.ICacheService cacheService)
        {
            _userRepository     = repo;
            _mapper             = mapper;
            _passwordHasher     = hasher;
            _jwtGenerator       = jwt;
            _fileStorageService = fileStorageService;
            _cacheService       = cacheService;
        }

        public async Task<IEnumerable<UserResDto>> GetAllUsersAsync(CurrentUser currentUser)
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResDto>>(users);
        }

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

            // 1.5 Handle File Upload
            string? savedPath = null;
            if (!string.IsNullOrEmpty(userDto.ProfileImageUrl)) // Check the new DTO property
            {
                try
                {
                    // Copy the file from the local path and get the relative path
                    savedPath = await _fileStorageService.CopyAndSaveFileAsync(userDto.ProfileImageUrl, "users/profiles");
                }
                catch (FileNotFoundException)
                {
                    // Let the controller decide how to respond to missing files
                    throw;
                }
                catch (Exception)
                {
                    // Rethrow so controller can return 500
                    throw;
                }
            }

            // 2. Map DTO to Entity and Hash Password
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);

            // Set the saved path on the model only when available, otherwise prefer DTO URL, otherwise default
            if (!string.IsNullOrEmpty(savedPath))
            {
                user.ProfileImageUrl = savedPath;
            }
            else
            {
                user.ProfileImageUrl = null;
            }

            var newUser = await _userRepository.AddAsync(user);
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
        /// Updates an existing user's details, including mapping contacts and handling the password/profile image path.
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
            var userByUserName = await _userRepository.GetByUserNameAsync(userDto.UserName);
            if (userByUserName != null && userByUserName.Id != id)
            {
                // Conflict: Another user already has this username (email)
                return null;
            }

            // Handle File Copy/Replacement using local path string
            if (!string.IsNullOrEmpty(userDto.ProfileImageUrl))
            {
                try
                {
                    // Delete the old file if one exists
                    if (!string.IsNullOrEmpty(userToUpdate.ProfileImageUrl))
                    {
                        _fileStorageService.DeleteFile(userToUpdate.ProfileImageUrl);
                    }

                    // Copy the file from the local path and save the new path
                    var copied = await _fileStorageService.CopyAndSaveFileAsync(
                        userDto.ProfileImageUrl,
                        "users/profiles"
                    );

                    if (!string.IsNullOrEmpty(copied))
                    {
                        userDto.ProfileImageUrl = copied;
                    }
                }
                catch (FileNotFoundException)
                {
                    // Let controller decide how to respond
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else 
            {
                userToUpdate.ProfileImageUrl = null;
            }

                // Map incoming DTO into existing entity (this will set flat properties)
                _mapper.Map(userDto, userToUpdate);

            // 3. Handle password change
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                userToUpdate.PasswordHash = _passwordHasher.HashPassword(userDto.Password);
            }

            var data = await _userRepository.UpdateAsync(userToUpdate);
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

            // 1. Delete the associated file from storage
            if (!string.IsNullOrEmpty(userToDelete.ProfileImageUrl))
            {
                _fileStorageService.DeleteFile(userToDelete.ProfileImageUrl);
            }

            // 2. Perform the permanent deletion
            await _userRepository.DeleteAsync(userToDelete);
            return true;
        }
    }
}
