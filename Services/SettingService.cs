using AutoMapper;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Data;

namespace pos_service.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public SettingService(ISettingRepository settingRepository, IMapper mapper, AppDbContext context)
        {
            _settingRepository = settingRepository;
            _mapper = mapper;
            _context = context;
        }

        // Settings are read-only via API: creation, update and deletion are not supported.
        public async Task<IEnumerable<Setting>> GetAllAsync(CurrentUser currentUser)
        {
            return await _settingRepository.GetAllAsync();
        }

        public async Task<Setting?> GetByIdAsync(int id, CurrentUser currentUser)
        {
            return await _settingRepository.GetByIdAsync(id);
        }

        public async Task<Setting?> GetByKeyAsync(SettingKey key, CurrentUser currentUser)
        {
            return await _settingRepository.GetByKeyAsync(key);
        }

        public async Task<Setting?> SetSettingValueAsync(SettingKey key, bool value, CurrentUser currentUser)
        {
            // Only allow system admin or roles with permission to view settings to toggle (we'll rely on controller permission)
            var setting = await _settingRepository.GetByKeyAsync(key);
            if (setting == null) return null;

            setting.SettingValue = value;
            var updated = await _settingRepository.UpdateAsync(setting);
            return updated;
        }
    }
}
