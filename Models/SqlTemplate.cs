using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class SqlTemplate : IAuditable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string TemplateName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string SqlQuery { get; set; }

        public string? PlaceholdersJson { get; set; }

        public string? SelectValuesJson { get; set; }



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
