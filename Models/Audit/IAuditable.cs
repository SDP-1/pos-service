using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.Audit
{
    public interface IAuditable
    {
        string Uuid         { get; set; }
        DateTime CreatedAt  { get; set; }
        DateTime? UpdatedAt { get; set; }
        string CreatedBy    { get; set; }
        string? UpdatedBy   { get; set; }
        bool IsActive       { get; set; }
    }
}
