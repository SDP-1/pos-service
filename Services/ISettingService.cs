using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Services
{
    public interface ISettingService
    {
        Task<IEnumerable<Setting>> GetAllAsync(CurrentUser currentUser);
        Task<Setting?> GetByKeyAsync(SettingKey key, CurrentUser currentUser);
        Task<Setting?> SetSettingValueAsync(SettingKey key, bool value, CurrentUser currentUser);

        // Convenience method that returns the boolean value for the given setting key.
        // The setting is expected to exist; if not found an exception will be thrown.
        Task<bool> GetSettingValueAsync(SettingKey key, CurrentUser currentUser);
    }
}
