using pos_service.Models.Enums;

namespace pos_service.Models.DTO.InventoryBatches
{
    /// <summary>
    /// Lightweight Data Transfer Object used for batch selection dropdowns in billing / POS and inventory adjustment workflows.
    /// Provides batch identifiers, remaining stock, active price points, and FIFO recommendation flags.
    /// </summary>
    public class InventoryBatchSelectDto
    {
        /// <summary>
        /// Globally unique identifier (UUID) for this batch.
        /// </summary>
        public string Uuid                              { get; set; } = string.Empty;

        /// <summary>
        /// The batch number identifier displayed in the dropdown.
        /// </summary>
        public string BatchNumber                       { get; set; } = string.Empty;

        /// <summary>
        /// Remaining stock quantity available in this batch.
        /// </summary>
        public decimal RemainingQuantity                { get; set; }

        /// <summary>
        /// Unit purchasing cost price for this batch.
        /// </summary>
        public decimal CostPrice                        { get; set; }

        /// <summary>
        /// Marked price / MRP for this batch.
        /// </summary>
        public decimal MarkedPrice                      { get; set; }

        /// <summary>
        /// Selling price for retail transactions.
        /// </summary>
        public decimal RetailPrice                      { get; set; }

        /// <summary>
        /// Selling price for wholesale transactions.
        /// </summary>
        public decimal WholesalePrice                   { get; set; }

        /// <summary>
        /// Discount percentage ratio for retail pricing.
        /// </summary>
        public decimal RetailDiscountRatio              { get; set; }

        /// <summary>
        /// Discount percentage ratio for wholesale pricing.
        /// </summary>
        public decimal WholesaleDiscountRatio           { get; set; }

        /// <summary>
        /// Optional invoice or reference information.
        /// </summary>
        public string? Reference                        { get; set; }

        /// <summary>
        /// Creation timestamp of this batch.
        /// </summary>
        public DateTime CreatedAt                       { get; set; }

        /// <summary>
        /// Indicates if this batch is recommended by FIFO / stock rotation logic.
        /// </summary>
        public bool IsRecommended                       { get; set; } = false;
    }
}
