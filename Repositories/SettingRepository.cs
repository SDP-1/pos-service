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

        public async Task<Setting> AddAsync(Setting setting)
        {
            setting.Uuid = Guid.NewGuid().ToString();
            _context.Settings.Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task DeleteAsync(Setting setting)
        {
            _context.Settings.Remove(setting);
            await _context.SaveChangesAsync();
        }

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

        public async Task<Setting?> GetByUuidAsync(string uuid)
        {
            return await _context.Settings.FirstOrDefaultAsync(s => s.Uuid == uuid && s.IsActive);
        }

        public async Task<Setting> UpdateAsync(Setting setting)
        {
            _context.Entry(setting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return setting;
        }
    }
}
