using pos_service.Models;

namespace pos_service.Repositories
{
    public interface IBackupLocationRepository
    {
        /// <summary>
        /// Retrieves all backup locations.
        /// </summary>
        /// <returns>Collection of BackupLocation entities.</returns>
        Task<IEnumerable<BackupLocation>> GetAllAsync();

        /// <summary>
        /// Finds a backup location by database id.
        /// </summary>
        /// <param name="id">Database id of the backup location.</param>
        /// <returns>The BackupLocation when found; otherwise null.</returns>
        Task<BackupLocation?> GetByIdAsync(int id);

        /// <summary>
        /// Finds an active backup location by UUID.
        /// </summary>
        /// <param name="uuid">UUID identifier of the backup location.</param>
        /// <returns>The BackupLocation when found and active; otherwise null.</returns>
        Task<BackupLocation?> GetByUuidAsync(string uuid);

        /// <summary>
        /// Adds a new backup location and assigns a UUID. If marked as default, will unset other defaults.
        /// </summary>
        /// <param name="loc">BackupLocation entity to add.</param>
        /// <returns>The added BackupLocation with identity fields populated.</returns>
        Task<BackupLocation> AddAsync(BackupLocation loc);

        /// <summary>
        /// Updates an existing backup location. If marked as default, will unset other defaults.
        /// </summary>
        /// <param name="loc">BackupLocation entity with updated values.</param>
        /// <returns>The updated BackupLocation.</returns>
        Task<BackupLocation> UpdateAsync(BackupLocation loc);

        /// <summary>
        /// Deletes the specified backup location. Prevents deleting the last remaining active location.
        /// </summary>
        /// <param name="loc">BackupLocation entity to remove.</param>
        Task DeleteAsync(BackupLocation loc);
    }
}
