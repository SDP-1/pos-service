using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Inventory
{
    public class InventoryUnitReqDto
    {
        [Required]
        public UnitType UnitType           { get; set; }

        public UnitType ParentUnitType     { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal QuantityPerParent   { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal QuantityInBaseUnits { get; set; }

        /// <summary>
        /// Indicates whether this unit represents the item's primary base unit of measure.
        /// </summary>
        public bool IsBaseUnit             { get; set; } = false;
    }
}
