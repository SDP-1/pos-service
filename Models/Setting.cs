using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models
{
    public class Setting : IAuditable
    {
        public int Id                { get; set; }

        [Required]
        public SettingKey SettingKey { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingName    { get; set; }

        /// <summary>
        /// Store the value as boolean as requested.
        /// </summary>
        public bool SettingValue     { get; set; }

        [MaxLength(500)]
        public string? Description   { get; set; }

        // --- IAuditable ---
        public string Uuid           { get; set; }
        public DateTime CreatedAt    { get; set; }
        public DateTime? UpdatedAt   { get; set; }
        public string CreatedBy      { get; set; }
        public string? UpdatedBy     { get; set; }
        public bool IsActive         { get; set; } = true;
    }
}
