using pos_service.Models.DTO.Backup;

namespace pos_service.Services.Backup
{
    public interface IBackupService
    {
        /// <summary>
        /// Creates a backup using the saved default or last-used location.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>BackupResponseDto with result details.</returns>
        Task<BackupResponseDto> CreateBackupAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a backup to the specified location or path.
        /// </summary>
        /// <param name="scheduleUuid">Optional schedule identifier for automated backups.</param>
        /// <param name="locationUuid">Optional UUID of the backup location to use.</param>
        /// <param name="targetPath">Optional explicit file system path to write the backup file.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>BackupResponseDto with result details.</returns>
        Task<BackupResponseDto> CreateBackupAsync(string? scheduleUuid, string? locationUuid, string? targetPath = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates or updates a backup location record from the DTO and returns the persistent BackupLocation.
        /// </summary>
        /// <param name="dto">BackupLocationDto containing properties for the location.</param>
        /// <returns>The created or updated BackupLocation entity, or null when dto is null.</returns>
        Task<Models.BackupLocation?> SaveOrGetLocationAsync(Models.DTO.Backup.BackupLocationDto dto);
    }
}
