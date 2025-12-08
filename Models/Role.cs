using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Role : IAuditable
    {
        public int Id              { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name         { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        // --- Implementation of IAuditable ---
        public string Uuid         { get; set; }
        public DateTime CreatedAt  { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy    { get; set; }
        public string? UpdatedBy   { get; set; }
        public bool IsActive       { get; set; } = true;
    }
}
