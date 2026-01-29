using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Backup
{
    public class ScheduleDto
    {
        // Schedule unique id
        public string? Uuid { get; set; }
        [Required]
        // Cron-like expression or simple time like "15:00" or interval in minutes like "PT30M"
        public string Schedule { get; set; }

        // Optional friendly name
        public string? Name { get; set; }

        // Enable or disable
        public bool Enabled { get; set; } = true;
        // Optional BackupLocation UUID assigned to this schedule
        public string? BackupLocationUuid { get; set; }
    }
}
