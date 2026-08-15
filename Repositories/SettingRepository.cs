using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public class SettingRepository : BaseRepository, ISettingRepository
    {
        private readonly ILogger<SettingRepository> _logger;

        public SettingRepository(AppDbContext context, ILogger<SettingRepository> logger) : base(context)
        {
            _logger = logger;
        }

        // Creation and deletion of settings are not supported via repository for now.

        // Creation and deletion of settings are not supported via repository for now.

        /// <summary>
        /// Retrieves all setting entries currently available.
        /// </summary>
        /// <returns>Collection of Setting entities.</returns>
        public async Task<IEnumerable<Setting>> GetAllAsync()
        {
            return await _context.Settings.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Finds an active setting by its key.
        /// </summary>
        /// <param name="key">The SettingKey enum value to lookup.</param>
        /// <returns>The Setting when found and active; otherwise null.</returns>
        public async Task<Setting?> GetByKeyAsync(SettingKey key)
        {
            return await _context.Settings.FirstOrDefaultAsync(s => s.SettingKey == key && s.IsActive);
        }

        /// <summary>
        /// Updates a setting value.
        /// </summary>
        /// <param name="setting">The Setting entity with updated values.</param>
        /// <returns>The updated Setting entity.</returns>
        public async Task<Setting> UpdateAsync(Setting setting)
        {
            _context.Entry(setting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return setting;
        }

        // UUID lookup and update are intentionally omitted to make settings read-only via API.
    }
}
