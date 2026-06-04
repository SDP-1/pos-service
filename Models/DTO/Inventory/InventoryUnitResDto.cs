using pos_service.Models.DTO.Audits;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Inventory
{
    public class InventoryUnitResDto : IFullResAuditDto
    {
        [Required]
        public UnitType UnitType           { get; set; }

        public UnitType ParentUnitType     { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal QuantityPerParent   { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal QuantityInBaseUnits { get; set; }

        public string Uuid                 { get; set; }
        public DateTime CreatedAt          { get; set; }
        public DateTime? UpdatedAt         { get; set; }
        public string CreatedBy            { get; set; }
        public string? UpdatedBy           { get; set; }
        public bool IsActive               { get; set; }
    }
}
