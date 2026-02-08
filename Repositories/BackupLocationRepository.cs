using Microsoft.EntityFrameworkCore;
using pos_service.Data;
using pos_service.Models;

namespace pos_service.Repositories
{
    public class BackupLocationRepository : IBackupLocationRepository
    {
        private readonly AppDbContext _context;

        public BackupLocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BackupLocation> AddAsync(BackupLocation loc)
        {
            loc.Uuid = Guid.NewGuid().ToString();
            // If this location is marked default, unset IsDefault on other locations
            if (loc.IsDefault)
            {
                var others = await _context.Set<BackupLocation>().Where(l => l.IsDefault).ToListAsync();
                foreach (var o in others)
                {
                    o.IsDefault = false;
                    _context.Entry(o).State = EntityState.Modified;
                }
            }

            _context.Add(loc);
            await _context.SaveChangesAsync();
            return loc;
        }

        public async Task DeleteAsync(BackupLocation loc)
        {
            // Prevent deleting the last remaining active location
            var activeCount = await _context.Set<BackupLocation>().CountAsync(l => l.IsActive && l.Id != loc.Id);
            if (activeCount <= 0)
            {
                throw new InvalidOperationException("At least one backup location must exist");
            }

            _context.Remove(loc);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BackupLocation>> GetAllAsync()
        {
            return await _context.Set<BackupLocation>().AsNoTracking().ToListAsync();
        }

        public async Task<BackupLocation?> GetByIdAsync(int id)
        {
            return await _context.Set<BackupLocation>().FindAsync(id);
        }

        public async Task<BackupLocation?> GetByUuidAsync(string uuid)
        {
            return await _context.Set<BackupLocation>().FirstOrDefaultAsync(l => l.Uuid == uuid && l.IsActive);
        }

        public async Task<BackupLocation> UpdateAsync(BackupLocation loc)
        {
            // If updated location is set as default, unset other defaults
            if (loc.IsDefault)
            {
                var others = await _context.Set<BackupLocation>().Where(l => l.IsDefault && l.Uuid != loc.Uuid).ToListAsync();
                foreach (var o in others)
                {
                    o.IsDefault = false;
                    _context.Entry(o).State = EntityState.Modified;
                }
            }

            _context.Entry(loc).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return loc;
        }
    }
}
