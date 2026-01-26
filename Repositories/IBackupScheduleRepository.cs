using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IBackupScheduleRepository
    {
        Task<IEnumerable<BackupSchedule>> GetAllAsync();
        Task<IEnumerable<BackupSchedule>> GetEnabledAsync();
        Task<BackupSchedule?> GetByIdAsync(int id);
        Task<BackupSchedule?> GetByUuidAsync(string uuid);
        Task<BackupSchedule> AddAsync(BackupSchedule schedule);
        Task<BackupSchedule> UpdateAsync(BackupSchedule schedule);
        Task DeleteAsync(BackupSchedule schedule);
    }
}
