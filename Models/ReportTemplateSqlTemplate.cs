using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class ReportTemplateSqlTemplate : IAuditable
    {
        public int Id { get; set; }

        [Required]
        public int ReportTemplateId { get; set; }
        public virtual ReportTemplate ReportTemplate { get; set; }

        [Required]
        public int SqlTemplateId { get; set; }
        public virtual SqlTemplate SqlTemplate { get; set; }

        // --- IAuditable ---
        [MaxLength(36)]
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(36)]
        public string? CreatedBy { get; set; }
        [MaxLength(36)]
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
