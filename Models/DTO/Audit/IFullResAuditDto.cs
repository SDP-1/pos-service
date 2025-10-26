namespace pos_service.Models.DTO.Audit
{
    public interface IFullResAuditDto
    {
        Guid Uuid           { get; set; }
        DateTime CreatedAt  { get; set; }
        DateTime? UpdatedAt { get; set; }
        string CreatedBy    { get; set; }
        string? UpdatedBy   { get; set; }
        bool IsActive       { get; set; }
    }
}
