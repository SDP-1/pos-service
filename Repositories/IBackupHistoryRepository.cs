using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IBackupHistoryRepository
    {
        Task<IEnumerable<BackupHistory>> GetAllAsync(int maxRecords = 50);
        Task<BackupHistory> AddAsync(BackupHistory history);
    }
}
