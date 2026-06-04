using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Inventory
{
    /// <summary>
    /// Response DTO representing a single inventory adjustment audit record.
    /// </summary>
    public class InventoryAdjustAuditResDto
    {
        public string InventoryUuid       { get; set; }

        public string ItemUuid            { get; set; }

        /// <summary>
        /// Stock quantity before the adjustment.
        /// </summary>
        public decimal PreviousQuantity   { get; set; }

        /// <summary>
        /// Stock quantity after the adjustment.
        /// </summary>
        public decimal NewQuantity        { get; set; }

        /// <summary>
        /// Amount of adjustment (positive for increase, negative for decrease).
        /// </summary>
        public decimal AdjustmentQuantity { get; set; }

        /// <summary>
        /// Unit type for the adjustment.
        /// </summary>
        public string UnitType            { get; set; }

        /// <summary>
        /// Whether the adjustment was an increase or decrease.
        /// </summary>
        public string AdjustmentType      { get; set; } // "Increase" or "Decrease"

        /// <summary>
        /// Optional comment about the adjustment.
        /// </summary>
        public string? Comment            { get; set; }

        /// <summary>
        /// Optional reason for the adjustment (especially for decreases).
        /// </summary>
        public string? Reason             { get; set; }

        /// <summary>
        /// Timestamp when the adjustment was made.
        /// </summary>
        public DateTime UpdatedAt         { get; set; }

        /// <summary>
        /// Username of the user who made the adjustment.
        /// </summary>
        public string? UpdatedByUser      { get; set; }

        /// <summary>
        /// UUID of the user who made the adjustment.
        /// </summary>
        public string? UpdatedBy          { get; set; }
    }
}
