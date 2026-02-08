namespace pos_service.Models.DTO.Backup
{
    public class BackupLocationDto
    {
        public string? Uuid { get; set; }
        public string? Name { get; set; }
        public string Path { get; set; } = string.Empty;
        public bool IsRemote { get; set; }
        public bool IsDefault { get; set; }
    }
}
