using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class BackupScheduleRepository : IBackupScheduleRepository
    {
        private readonly AppDbContext _context;

        public BackupScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BackupSchedule> AddAsync(BackupSchedule schedule)
        {
            schedule.Uuid = Guid.NewGuid().ToString();
            _context.BackupSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task DeleteAsync(BackupSchedule schedule)
        {
            _context.BackupSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BackupSchedule>> GetAllAsync()
        {
            return await _context.BackupSchedules
                .Include(s => s.BackupLocation)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BackupSchedule?> GetByIdAsync(int id)
        {
            return await _context.BackupSchedules.FindAsync(id);
        }

        public async Task<BackupSchedule?> GetByUuidAsync(string uuid)
        {
            return await _context.BackupSchedules.FirstOrDefaultAsync(s => s.Uuid == uuid && s.IsActive);
        }

        public async Task<IEnumerable<BackupSchedule>> GetEnabledAsync()
        {
            return await _context.BackupSchedules
                .Include(s => s.BackupLocation)
                .Where(s => s.Enabled && s.IsActive)
                .ToListAsync();
        }

        public async Task<BackupSchedule> UpdateAsync(BackupSchedule schedule)
        {
            _context.Entry(schedule).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return schedule;
        }
    }
}
