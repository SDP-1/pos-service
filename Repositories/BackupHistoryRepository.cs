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

        public async Task<BackupHistory> AddAsync(BackupHistory history)
        {
            history.Uuid = Guid.NewGuid().ToString();
            _context.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

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
