using pos_service.Models.Audit;

namespace pos_service.Models
{
    public class BackupLocation : IAuditable
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty; // friendly name
        public string Path { get; set; } = string.Empty; // actual path or URI
        public bool IsRemote { get; set; } = false; // local vs remote
        public bool IsDefault { get; set; } = false; // whether this is default for manual backups

        // IAuditable
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
