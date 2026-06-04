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

        /// <summary>
        /// Adds a new backup location and assigns a UUID. If marked as default, will unset other defaults.
        /// </summary>
        /// <param name="loc">BackupLocation entity to add.</param>
        /// <returns>The added BackupLocation with identity fields populated.</returns>
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

        /// <summary>
        /// Deletes the specified backup location. Prevents deleting the last remaining active location.
        /// </summary>
        /// <param name="loc">BackupLocation entity to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown when attempting to remove the last active location.</exception>
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

        /// <summary>
        /// Retrieves all backup locations.
        /// </summary>
        /// <returns>Collection of BackupLocation entities.</returns>
        public async Task<IEnumerable<BackupLocation>> GetAllAsync()
        {
            return await _context.Set<BackupLocation>().AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Finds a backup location by database id.
        /// </summary>
        /// <param name="id">Database id of the backup location.</param>
        /// <returns>The BackupLocation when found; otherwise null.</returns>
        public async Task<BackupLocation?> GetByIdAsync(int id)
        {
            return await _context.Set<BackupLocation>().FindAsync(id);
        }

        /// <summary>
        /// Finds an active backup location by UUID.
        /// </summary>
        /// <param name="uuid">UUID identifier of the backup location.</param>
        /// <returns>The BackupLocation when found and active; otherwise null.</returns>
        public async Task<BackupLocation?> GetByUuidAsync(string uuid)
        {
            return await _context.Set<BackupLocation>().FirstOrDefaultAsync(l => l.Uuid == uuid && l.IsActive);
        }

        /// <summary>
        /// Updates an existing backup location. If marked as default, will unset other defaults.
        /// </summary>
        /// <param name="loc">BackupLocation entity with updated values.</param>
        /// <returns>The updated BackupLocation.</returns>
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
