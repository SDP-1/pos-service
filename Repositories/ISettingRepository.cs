using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public interface ISettingRepository
    {
        Task<IEnumerable<Setting>> GetAllAsync();
        Task<Setting?> GetByIdAsync(int id);
        Task<Setting?> GetByUuidAsync(string uuid);
        Task<Setting?> GetByKeyAsync(SettingKey key);
        Task<Setting> AddAsync(Setting setting);
        Task<Setting> UpdateAsync(Setting setting);
        Task DeleteAsync(Setting setting);
    }
}
