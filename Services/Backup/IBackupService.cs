using pos_service.Models.DTO.Backup;

namespace pos_service.Services.Backup
{
    public interface IBackupService
    {
        Task<BackupResponseDto> CreateBackupAsync(CancellationToken cancellationToken = default);
        Task<BackupResponseDto> CreateBackupAsync(string? scheduleUuid, string? locationUuid, string? targetPath = null, CancellationToken cancellationToken = default);
        Task<Models.BackupLocation?> SaveOrGetLocationAsync(Models.DTO.Backup.BackupLocationDto dto);
    }
}
