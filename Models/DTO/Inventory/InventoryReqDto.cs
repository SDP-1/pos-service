using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Inventory
{
    public class InventoryReqDto
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal StockQuantity { get; set; }

        public bool AllowsDecimalQuantities { get; set; }

        [Required]
        public UnitType UnitType { get; set; }

        public List<InventoryUnitReqDto> Units { get; set; } = new();

        /// <summary>
        /// Optional hierarchical packaging definitions (e.g., Pack contains 12 Bottle, Box contains 3 Pack).
        /// When provided, Units will be recalculated from this structure using UnitType as the base unit.
        /// </summary>
        public List<PackagingLevelDto> PackagingLevels { get; set; } = new();

        // Expiries are managed during inventory adjustments, not during full inventory upserts.
    }
}
