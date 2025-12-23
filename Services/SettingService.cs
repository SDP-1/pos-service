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

        public async Task<Setting> CreateAsync(Setting setting, CurrentUser currentUser)
        {
            // basic validations
            var exists = await _settingRepository.GetByKeyAsync(setting.SettingKey);
            if (exists != null)
            {
                // update existing
                exists.SettingValue = setting.SettingValue;
                exists.Description = setting.Description;
                return await _settingRepository.UpdateAsync(exists);
            }

            var created = await _settingRepository.AddAsync(setting);
            return created;
        }

        public async Task<bool> DeleteAsync(int id, CurrentUser currentUser)
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            if (setting == null) return false;
            await _settingRepository.DeleteAsync(setting);
            return true;
        }

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

        public async Task<Setting> UpdateAsync(int id, Setting setting, CurrentUser currentUser)
        {
            var existing = await _settingRepository.GetByIdAsync(id);
            if (existing == null)
                throw new ArgumentException($"Setting with ID {id} not found");

            existing.Description = setting.Description;
            existing.SettingValue = setting.SettingValue;

            return await _settingRepository.UpdateAsync(existing);
        }
    }
}
