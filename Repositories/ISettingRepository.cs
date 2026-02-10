using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public interface ISettingRepository
    {
        Task<IEnumerable<Setting>> GetAllAsync();
        Task<Setting?> GetByIdAsync(int id);
        Task<Setting?> GetByKeyAsync(SettingKey key);
        Task<Setting> UpdateAsync(Setting setting);
    }
}
