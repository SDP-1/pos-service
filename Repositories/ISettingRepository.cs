using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public interface ISettingRepository
    {
        /// <summary>
        /// Retrieves all setting entries currently available.
        /// </summary>
        /// <returns>Collection of Setting entities.</returns>
        Task<IEnumerable<Setting>> GetAllAsync();

        /// <summary>
        /// Finds an active setting by its key.
        /// </summary>
        /// <param name="key">The SettingKey enum value to lookup.</param>
        /// <returns>The Setting when found and active; otherwise null.</returns>
        Task<Setting?> GetByKeyAsync(SettingKey key);

        /// <summary>
        /// Updates a setting value.
        /// </summary>
        /// <param name="setting">The Setting entity with updated values.</param>
        /// <returns>The updated Setting entity.</returns>
        Task<Setting> UpdateAsync(Setting setting);
    }
}
