using pos_service.Models.DTO.Audits;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    public class ItemExpiryResDto : IFullResAuditDto
    {
        [Required]
        public DateTime ExpDate     { get; set; }

        [Range(0, int.MaxValue)]
        public int NotifyBeforeDays { get; set; } = 0;

        public string Uuid          { get; set; }
        public DateTime CreatedAt   { get; set; }
        public DateTime? UpdatedAt  { get; set; }
        public string CreatedBy     { get; set; }
        public string? UpdatedBy    { get; set; }
        public bool IsActive        { get; set; }
    }
}
