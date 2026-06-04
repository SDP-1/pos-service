using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class BackupHistoryRepository : IBackupHistoryRepository
    {
        private readonly AppDbContext _context;

        public BackupHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new backup history record and assigns a UUID.
        /// </summary>
        /// <param name="history">BackupHistory entity to add.</param>
        /// <returns>The added BackupHistory entity with identity fields populated.</returns>
        public async Task<BackupHistory> AddAsync(BackupHistory history)
        {
            history.Uuid = Guid.NewGuid().ToString();
            _context.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        /// <summary>
        /// Retrieves recent backup history records ordered by execution time descending.
        /// </summary>
        /// <param name="maxRecords">Maximum number of records to return. Defaults to 50 when not provided or invalid.</param>
        /// <returns>A collection of BackupHistory records.</returns>
        public async Task<IEnumerable<BackupHistory>> GetAllAsync(int maxRecords = 50)
        {
            if (maxRecords <= 0) maxRecords = 50;
            return await _context.Set<BackupHistory>()
                .AsNoTracking()
                .OrderByDescending(h => h.ExecutedAt)
                .Take(maxRecords)
                .ToListAsync();
        }
    }
}
