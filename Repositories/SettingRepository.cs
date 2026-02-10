using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;
using pos_service.Models.Enums;

namespace pos_service.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SettingRepository> _logger;

        public SettingRepository(AppDbContext context, ILogger<SettingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Creation and deletion of settings are not supported via repository for now.

        public async Task<IEnumerable<Setting>> GetAllAsync()
        {
            return await _context.Settings.AsNoTracking().ToListAsync();
        }

        public async Task<Setting?> GetByIdAsync(int id)
        {
            return await _context.Settings.FindAsync(id);
        }

        public async Task<Setting?> GetByKeyAsync(SettingKey key)
        {
            return await _context.Settings.FirstOrDefaultAsync(s => s.SettingKey == key && s.IsActive);
        }

        public async Task<Setting> UpdateAsync(Setting setting)
        {
            _context.Entry(setting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return setting;
        }

        // UUID lookup and update are intentionally omitted to make settings read-only via API.
    }
}
