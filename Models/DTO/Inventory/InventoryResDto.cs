using pos_service.Models.Enums;
using pos_service.Models.DTO.Items;

namespace pos_service.Models.DTO.Inventory
{
    /// <summary>
    /// Response DTO representing the inventory stock, packaging unit hierarchy, and batch summary of an item.
    /// </summary>
    public class InventoryResDto
    {
        /// <summary>
        /// Unique UUID of the associated product item.
        /// </summary>
        public string ItemUuid                 { get; set; }

        /// <summary>
        /// Total remaining available stock quantity in base inventory units.
        /// </summary>
        public decimal StockQuantity           { get; set; }

        /// <summary>
        /// Indicates whether non-integer / decimal quantities are permitted for sales and stock adjustments.
        /// </summary>
        public bool AllowsDecimalQuantities    { get; set; }

        /// <summary>
        /// Base unit of measure for this item (e.g., Each, Kilogram, Litre).
        /// </summary>
        public UnitType UnitType               { get; set; }

        /// <summary>
        /// Multi-level packaging unit hierarchy definitions.
        /// </summary>
        public List<InventoryUnitResDto> Units { get; set; } = new();

        /// <summary>
        /// Active price tier configuration for the item.
        /// </summary>
        public ItemPriceResDto Price           { get; set; } = new();

        /// <summary>
        /// List of tracked expiration dates and notification thresholds.
        /// </summary>
        public List<ItemExpiryResDto> Expiries { get; set; } = new();

        /// <summary>
        /// Total count of active batch lots currently existing for this item.
        /// </summary>
        public int BatchCount                  { get; set; }

        /// <summary>
        /// Unique UUID identifier of the inventory record.
        /// </summary>
        public string Uuid                     { get; set; }

        /// <summary>
        /// Creation timestamp in UTC.
        /// </summary>
        public DateTime CreatedAt              { get; set; }

        /// <summary>
        /// Last modification timestamp in UTC.
        /// </summary>
        public DateTime? UpdatedAt             { get; set; }

        /// <summary>
        /// User UUID or display name of the creator.
        /// </summary>
        public string CreatedBy                { get; set; }

        /// <summary>
        /// User UUID or display name of the last modifier.
        /// </summary>
        public string? UpdatedBy               { get; set; }

        /// <summary>
        /// Soft deletion / active status flag.
        /// </summary>
        public bool IsActive                   { get; set; }
    }
}
