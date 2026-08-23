using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an item packaging unit / measurement level in <c>tbl_item_units</c>.
    /// Defines conversion factors, base unit flags, parent packaging relationships, and unit quantities for multi-level inventory packaging.
    /// </summary>
    [Table("tbl_item_units")]
    public class ItemUnit
    {
        /// <summary>
        /// The unique primary key identifier for the item unit packaging record.
        /// </summary>
        public int Id                      { get; set; }

        /// <summary>
        /// The unique identifier (UUID) of the parent item.
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string ItemUuid             { get; set; }

        /// <summary>
        /// The packaging unit type of this level (e.g., Item, Pack, Box, Carton, Container).
        /// </summary>
        public UnitType UnitType           { get; set; }

        /// <summary>
        /// The parent unit in the packaging hierarchy (e.g., Pack -> Item).
        /// </summary>
        public UnitType? ParentUnitType    { get; set; }

        /// <summary>
        /// How many parent units make one of this unit (e.g., Pack has 3 Item).
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal? QuantityPerParent  { get; set; }

        /// <summary>
        /// Number of base units represented by one unit of this type.
        /// Example: if base is Bottle and Pack contains 12 bottles, store 12.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal QuantityInBaseUnits { get; set; }

        /// <summary>
        /// Indicates whether this unit is the base measurement unit for the item.
        /// </summary>
        public bool IsBaseUnit             { get; set; } = false;

        /// <summary>
        /// Navigation property to the parent item entity.
        /// </summary>
        public virtual Item Item           { get; set; }

        /// <summary>
        /// Globally unique identifier (UUID) for this unit record.
        /// </summary>
        public string Uuid                 { get; set; }
    }
}
