using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public interface ISettingService
    {
        Task<IEnumerable<Setting>> GetAllAsync(CurrentUser currentUser);
        Task<Setting?> GetByIdAsync(int id, CurrentUser currentUser);
        Task<Setting?> GetByKeyAsync(SettingKey key, CurrentUser currentUser);
        Task<Setting?> SetSettingValueAsync(SettingKey key, bool value, CurrentUser currentUser);
    }
}
