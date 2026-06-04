using pos_service.Models;
using pos_service.Models.DTO.Roles;

namespace pos_service.Services.Roles
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResDto>> GetAllAsync();
        Task<IEnumerable<RoleResDto>> GetActiveAsync();
        Task<RoleResDto?> GetByIdAsync(int id);
        Task<RoleResDto?> CreateAsync(RoleReqDto role);
        Task<RoleResDto?> UpdateAsync(int id, RoleReqDto role);
        Task<bool> DeleteAsync(int id);
        Task<RoleResDto?> SetActiveStatusAsync(int id, bool isActive);
    }
}