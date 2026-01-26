using pos_service.Models.Audit;

namespace pos_service.Models
{
    public class BackupSchedule : IAuditable
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string Schedule { get; set; } = string.Empty; // HH:mm or PT30M
        public bool Enabled { get; set; } = true;
        public string? BackupPath { get; set; }
        public int? RetentionDays { get; set; }
        public DateTime? LastRunAt { get; set; }
        // Primary location for this schedule (UUID of BackupLocation)
        // Navigation: many-to-one to BackupLocation (each schedule uses one location)
        public string? BackupLocationUuid { get; set; }
        public BackupLocation? BackupLocation { get; set; }

        // IAuditable
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
