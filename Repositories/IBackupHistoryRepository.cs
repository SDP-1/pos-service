using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IBackupHistoryRepository
    {
        /// <summary>
        /// Retrieves recent backup history records ordered by execution time descending.
        /// </summary>
        /// <param name="maxRecords">Maximum number of records to return. Defaults to 50 when not provided or invalid.</param>
        /// <returns>A collection of BackupHistory records.</returns>
        Task<IEnumerable<BackupHistory>> GetAllAsync(int maxRecords = 50);

        /// <summary>
        /// Adds a new backup history record and assigns a UUID.
        /// </summary>
        /// <param name="history">BackupHistory entity to add.</param>
        /// <returns>The added BackupHistory entity with identity fields populated.</returns>
        Task<BackupHistory> AddAsync(BackupHistory history);
    }
}
