using AutoMapper;
using Microsoft.AspNetCore.Identity;
using pos_service.Models;
using pos_service.Models.DTO.User;
using pos_service.Repositories;
using pos_service.Security;
using pos_service.Services.Common;

namespace pos_service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository     _userRepository;
        private readonly IMapper             _mapper;
        private readonly IPasswordHasher     _passwordHasher;
        private readonly IJwtGenerator       _jwtGenerator;
        private readonly IFileStorageService _fileStorageService;
        public UserService(IUserRepository repo, IMapper mapper, IPasswordHasher hasher, IJwtGenerator jwt, IFileStorageService fileStorageService)
        {
            _userRepository     = repo;
            _mapper             = mapper;
            _passwordHasher     = hasher;
            _jwtGenerator       = jwt;
            _fileStorageService = fileStorageService;
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
                    // You may decide to ignore the file and continue, or return a BadRequest
                    // For now, let's treat it as an error
                    // To handle this properly, the Controller should catch a custom exception and return 400
                    return null;
                }
                catch (Exception ex)
                {
                    // Log the error (e.g., permission denied)
                    // To handle this properly, the Controller should catch a custom exception and return 500
                    return null;
                }
            }

            // 2. Map DTO to Entity and Hash Password
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);

            // Set the saved path on the model
            user.ProfileImageUrl = savedPath;

            var newUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserResDto>(newUser);
        }

        public async Task<bool> DeactivateUserAsync(int id, CurrentUser currentUser)
        {
            var userToUpdate = await _userRepository.GetByIdAsync(id);
            if (userToUpdate == null) return false;

            userToUpdate.IsActive = false;
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
            userToUpdate.UpdatedAt = DateTime.UtcNow;
            // userToUpdate.UpdatedBy = GetCurrentUserName(); // The user who performed the activation

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

            return (_mapper.Map<UserResDto>(user), token);
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
            user.UpdatedAt = DateTime.UtcNow;
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
                    userToUpdate.ProfileImageUrl = await _fileStorageService.CopyAndSaveFileAsync(
                        userDto.ProfileImageUrl,
                        "users/profiles"
                    );
                }
                catch (FileNotFoundException)
                {
                    // You should handle this in the controller by throwing an exception, 
                    // but for simplicity here, we can return false.
                    return null;
                }
                catch (Exception)
                {
                    // Handle other file system errors (e.g., permission denied)
                    return null;
                }
            }

            // 2. Map flat properties (FirstName, LastName, Role, NIC, ImageUrl)
            // NOTE: The ProfileImageUrl is set above, but other properties are mapped here.
            _mapper.Map(userDto, userToUpdate);

            // 3. Handle password change
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                userToUpdate.PasswordHash = _passwordHasher.HashPassword(userDto.Password);
            }

            // 4. Handle Auditable properties
            userToUpdate.UpdatedAt = DateTime.UtcNow;
            // userToUpdate.UpdatedBy = GetCurrentUserName();

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
