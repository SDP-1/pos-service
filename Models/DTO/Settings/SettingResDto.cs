using System.ComponentModel.DataAnnotations;
using pos_service.Models.DTO.Audits;
using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Settings
{
    public class SettingResDto : IFullResAuditDto
    {
        public int Id                { get; set; }

        [Required]
        public SettingKey SettingKey { get; set; }

        [Required]
        public string SettingName    { get; set; }

        public bool SettingValue     { get; set; }

        [Required]
        public SettingCategory Category { get; set; }

        public string? Description   { get; set; }

        // Audit fields
        public string Uuid           { get; set; }
        public DateTime CreatedAt    { get; set; }
        public DateTime? UpdatedAt   { get; set; }
        public string CreatedBy      { get; set; }
        public string? UpdatedBy     { get; set; }
        public bool IsActive         { get; set; }
    }
}
