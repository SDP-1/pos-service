using pos_service.Models.DTO.Audits;
using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Settings
{
    public class SettingResDto : IFullResAuditDto
    {
        public int Id                { get; set; }
        public SettingKey SettingKey { get; set; }
        public string SettingName    { get; set; }
        public bool SettingValue     { get; set; }
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
