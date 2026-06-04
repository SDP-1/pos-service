using pos_service.Models.Audit;

namespace pos_service.Models
{
    public class BackupHistory : IAuditable
    {
        public int Id              { get; set; }
        public string Uuid         { get; set; } = Guid.NewGuid().ToString();
        public string ScheduleUuid { get; set; } = string.Empty;
        public string LocationUuid { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public bool Success        { get; set; }
        public string Message      { get; set; } = string.Empty;
        public string FilePath     { get; set; } = string.Empty;

        // IAuditable
        public DateTime CreatedAt  { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy   { get; set; }
        public string? UpdatedBy   { get; set; }
        public bool IsActive       { get; set; } = true;
    }
}
