namespace pos_service.Models.DTO.Audits
{
    public interface IFullResAuditDto
    {
        string Uuid         { get; set; }
        DateTime CreatedAt  { get; set; }
        DateTime? UpdatedAt { get; set; }
        string CreatedBy    { get; set; }
        string? UpdatedBy   { get; set; }
        bool IsActive       { get; set; }
    }
}
