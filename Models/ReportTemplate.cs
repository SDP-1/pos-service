using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class ReportTemplate : IAuditable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ReportName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string HtmlContent { get; set; }

        public string? ParametersJson { get; set; }

        public string? SqlPlaceholderMappingsJson { get; set; }



        public virtual ICollection<ReportTemplateSqlTemplate> ReportTemplateSqlTemplates { get; set; } = new List<ReportTemplateSqlTemplate>();

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
