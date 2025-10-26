using pos_service.Models.DTO.User;

namespace pos_service.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResDto>> GetAllUsersAsync();
        Task<UserResDto?> GetUserByIdAsync(int id);
        Task<UserResDto?> CreateUserAsync(UserReqDto userDto);
        Task<bool> UpdateUserAsync(int id, UserReqDto userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> DeactivateUserAsync(int id);
        Task<bool> ActivateUserAsync(int id);

        // Authentication methods
        Task<(UserResDto? User, string? Token)> LoginAsync(UserLoginReqDto loginDto);
        Task<bool> ChangePasswordAsync(int id, string oldPassword, string newPassword);
    }
}
