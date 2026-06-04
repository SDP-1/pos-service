using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Inventory
{
    /// <summary>
    /// Describes a packaging level relative to a parent unit.
    /// Example: UnitType=Pack, ParentUnitType=Bottle, QuantityPerParent=12 means 1 Pack = 12 Bottles.
    /// </summary>
    public class PackagingLevelDto
    {
        [Required]
        public UnitType UnitType { get; set; }

        [Required]
        public UnitType ParentUnitType { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal QuantityPerParent { get; set; }
    }
}
