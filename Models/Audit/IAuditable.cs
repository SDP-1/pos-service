using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.Audit
{
    public interface IAuditable
    {
        [MaxLength(36)]
        string Uuid         { get; set; }
        DateTime CreatedAt  { get; set; }
        DateTime? UpdatedAt { get; set; }
        [MaxLength(36)]
        string? CreatedBy    { get; set; }
        [MaxLength(36)]
        string? UpdatedBy   { get; set; }
        bool IsActive       { get; set; }
    }
}
