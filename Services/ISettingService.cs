using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public interface ISettingService
    {
        Task<IEnumerable<Setting>> GetAllAsync(CurrentUser currentUser);
        Task<Setting?> GetByIdAsync(int id, CurrentUser currentUser);
        Task<Setting?> GetByKeyAsync(SettingKey key, CurrentUser currentUser);
        Task<Setting> CreateAsync(Setting setting, CurrentUser currentUser);
        Task<Setting> UpdateAsync(int id, Setting setting, CurrentUser currentUser);
        Task<bool> DeleteAsync(int id, CurrentUser currentUser);
    }
}
