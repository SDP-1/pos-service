namespace pos_service.Models.DTO.Backup
{
    public class BackupResponseDto
    {
        public bool Success        { get; set; }
        public string Message      { get; set; } = string.Empty;
        public string? FilePath    { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
