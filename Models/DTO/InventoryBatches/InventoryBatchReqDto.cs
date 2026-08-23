using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.InventoryBatches
{
    /// <summary>
    /// Request Data Transfer Object for creating or initializing a new inventory batch.
    /// Specifies batch quantity, unit type, full-spectrum pricing (cost, marked, retail, wholesale), supplier reference, and expiry information.
    /// </summary>
    public class InventoryBatchReqDto
    {
        /// <summary>
        /// The UUID of the item to which this batch belongs.
        /// </summary>
        [Required]
        public string ItemUuid                          { get; set; } = string.Empty;

        /// <summary>
        /// Optional custom batch number. If omitted, the system generates one automatically.
        /// </summary>
        public string? BatchNumber                      { get; set; }

        /// <summary>
        /// Initial stock quantity to receive into the batch.
        /// </summary>
        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity                         { get; set; }

        /// <summary>
        /// Packaging unit measurement type for the received quantity.
        /// </summary>
        public UnitType UnitType                        { get; set; } = UnitType.Each;

        // Pricing

        /// <summary>
        /// Unit purchasing cost price for items in this batch.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CostPrice                        { get; set; }

        /// <summary>
        /// Manufacturer marked price / MRP for items in this batch.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal MarkedPrice                      { get; set; }

        /// <summary>
        /// Retail selling price for this batch.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal RetailPrice                      { get; set; }

        /// <summary>
        /// Wholesale selling price for this batch.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal WholesalePrice                   { get; set; }

        /// <summary>
        /// Percentage discount applied to marked price for retail sales.
        /// </summary>
        [Range(0, 100)]
        public decimal RetailDiscountRatio              { get; set; } = 0.0m;

        /// <summary>
        /// Percentage discount applied to marked price for wholesale sales.
        /// </summary>
        [Range(0, 100)]
        public decimal WholesaleDiscountRatio           { get; set; } = 0.0m;

        /// <summary>
        /// Optional UUID of the supplier who provided this batch.
        /// </summary>
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// Optional UUID of the associated purchase order.
        /// </summary>
        public string? PurchaseUuid                     { get; set; }

        /// <summary>
        /// External invoice or reference document text.
        /// </summary>
        [MaxLength(200)]
        public string? Reference                        { get; set; }

        /// <summary>
        /// Reason for creating or initializing the batch.
        /// </summary>
        public string? Reason                           { get; set; }

        /// <summary>
        /// Additional notes or user comments.
        /// </summary>
        public string? Comment                          { get; set; }
    }
}
