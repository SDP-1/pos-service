using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class InventoryUnit : IAuditable
    {
        public int Id                      { get; set; }

        [Required]
        public int InventoryId             { get; set; }

        public UnitType UnitType           { get; set; }

        /// <summary>
        /// The parent unit in the packaging hierarchy (e.g., Pack -> Item).
        /// </summary>
        public UnitType ParentUnitType     { get; set; }

        /// <summary>
        /// How many parent units make one of this unit (e.g., Pack has 3 Item).
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal QuantityPerParent   { get; set; }

        /// <summary>
        /// Number of base units represented by one unit of this type.
        /// Example: if base is Bottle and Pack contains 12 bottles, store 12.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal QuantityInBaseUnits { get; set; }

        public virtual Inventory Inventory { get; set; }

        public string Uuid                 { get; set; }
        public DateTime CreatedAt          { get; set; }
        public DateTime? UpdatedAt         { get; set; }
        public string CreatedBy            { get; set; }
        public string? UpdatedBy           { get; set; }
        public bool IsActive               { get; set; } = true;
    }
}
