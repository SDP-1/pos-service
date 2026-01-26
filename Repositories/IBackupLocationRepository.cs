using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IBackupLocationRepository
    {
        Task<IEnumerable<BackupLocation>> GetAllAsync();
        Task<BackupLocation?> GetByIdAsync(int id);
        Task<BackupLocation?> GetByUuidAsync(string uuid);
        Task<BackupLocation> AddAsync(BackupLocation loc);
        Task<BackupLocation> UpdateAsync(BackupLocation loc);
        Task DeleteAsync(BackupLocation loc);
    }
}
