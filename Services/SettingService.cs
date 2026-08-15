using AutoMapper;
using pos_service.Models;
using pos_service.Models.Enums;
using pos_service.Repositories;
using pos_service.Data;
using pos_service.Services.Common.Cache;
using System.Linq;

namespace pos_service.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        private const CacheExpiry DefaultExpiry = CacheExpiry.OneDay;

        public SettingService(ISettingRepository settingRepository, IMapper mapper, ICacheService cache)
        {
            _settingRepository = settingRepository;
            _mapper            = mapper;
            _cache             = cache;
        }

        // Settings are read-only via API: creation, update and deletion are not supported.
        // Cache the full settings list to avoid hitting the database for every lookup.
        public async Task<IEnumerable<Setting>> GetAllAsync(CurrentUser currentUser)
        {
            return await _cache.GetOrCreateAsync<IEnumerable<Setting>>(ServiceCacheKey.Settings, null,
                () => _settingRepository.GetAllAsync(), DefaultExpiry);
        }

        public async Task<Setting?> GetByKeyAsync(SettingKey key, CurrentUser currentUser)
        {
            // Try cached list first
            var cached = _cache.Get<IEnumerable<Setting>>(ServiceCacheKey.Settings);
            if (cached != null)
                return cached.FirstOrDefault(s => s.SettingKey == key && s.IsActive);

            // Fallback to DB and then cache the full list
            var fromDb = await _settingRepository.GetByKeyAsync(key);
            var all    = await _settingRepository.GetAllAsync();
            _cache.Set(ServiceCacheKey.Settings, null, all, DefaultExpiry);

            return fromDb;
        }

        // GetByIdAsync intentionally omitted - settings should be accessed by key.

        public async Task<bool> GetSettingValueAsync(SettingKey key, CurrentUser currentUser)
        {
            // Try cached list first
            var cached = _cache.Get<IEnumerable<Setting>>(ServiceCacheKey.Settings);
            if (cached != null)
            {
                var s = cached.FirstOrDefault(s => s.SettingKey == key && s.IsActive);
                if (s != null) return s.SettingValue;
                throw new InvalidOperationException($"Setting not found: {key}");
            }

            // Fallback: attempt to get by key and cache all settings
            var setting = await _settingRepository.GetByKeyAsync(key);
            var all     = await _settingRepository.GetAllAsync();
            _cache.Set(ServiceCacheKey.Settings, null, all, DefaultExpiry);

            if (setting == null)
                throw new InvalidOperationException($"Setting not found: {key}");

            return setting.SettingValue;
        }

        public async Task<Setting?> SetSettingValueAsync(SettingKey key, bool value, CurrentUser currentUser)
        {
            // Only allow system admin or roles with permission to toggle (controller enforces permissions)
            var setting = await _settingRepository.GetByKeyAsync(key);
            if (setting == null) return null;

            setting.SettingValue = value;
            var updated = await _settingRepository.UpdateAsync(setting);

            // Invalidate settings cache so subsequent reads pick up the change
            _cache.RemovePrimary(ServiceCacheKey.Settings);

            return updated;
        }
    }
}
