using pos_service.Models.Enums;

namespace pos_service.Models.DTO.InventoryBatches
{
    /// <summary>
    /// Response Data Transfer Object representing an inventory batch.
    /// Exposes batch details including remaining quantity, pricing tiers, supplier link, audit stamps, and lifecycle status.
    /// </summary>
    public class InventoryBatchResDto
    {
        /// <summary>
        /// The unique primary key identifier of the batch.
        /// </summary>
        public int Id                                   { get; set; }

        /// <summary>
        /// Globally unique identifier (UUID) for this batch.
        /// </summary>
        public string Uuid                              { get; set; } = string.Empty;

        /// <summary>
        /// The UUID of the parent item.
        /// </summary>
        public string ItemUuid                          { get; set; } = string.Empty;

        /// <summary>
        /// The display name of the item.
        /// </summary>
        public string? ItemName                         { get; set; }

        /// <summary>
        /// The receipt print name of the item.
        /// </summary>
        public string? ItemPrintName                    { get; set; }

        /// <summary>
        /// The item barcode.
        /// </summary>
        public string? ItemBarcode                      { get; set; }

        /// <summary>
        /// Formatted composite item number (e.g., ITEM-0012-0).
        /// </summary>
        public string? ItemNumber                       { get; set; }

        /// <summary>
        /// The alphanumeric batch number identifier.
        /// </summary>
        public string BatchNumber                       { get; set; } = string.Empty;

        /// <summary>
        /// Total initial quantity received into this batch.
        /// </summary>
        public decimal ReceivedQuantity                 { get; set; }

        /// <summary>
        /// Current available remaining quantity in this batch.
        /// </summary>
        public decimal RemainingQuantity                { get; set; }

        /// <summary>
        /// Unit purchasing cost price for items in this batch.
        /// </summary>
        public decimal CostPrice                        { get; set; }

        /// <summary>
        /// Manufacturer marked price / MRP for items in this batch.
        /// </summary>
        public decimal MarkedPrice                      { get; set; }

        /// <summary>
        /// Selling price for regular retail transactions.
        /// </summary>
        public decimal RetailPrice                      { get; set; }

        /// <summary>
        /// Selling price for wholesale transactions.
        /// </summary>
        public decimal WholesalePrice                   { get; set; }

        /// <summary>
        /// Percentage discount ratio for retail sales.
        /// </summary>
        public decimal RetailDiscountRatio              { get; set; }

        /// <summary>
        /// Percentage discount ratio for wholesale sales.
        /// </summary>
        public decimal WholesaleDiscountRatio           { get; set; }

        /// <summary>
        /// External invoice or GRN reference.
        /// </summary>
        public string? Reference                        { get; set; }

        /// <summary>
        /// UUID of the originating purchase order, if applicable.
        /// </summary>
        public string? PurchaseUuid                     { get; set; }

        /// <summary>
        /// UUID of the supplier who provided the batch.
        /// </summary>
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// Name of the supplier.
        /// </summary>
        public string? SupplierName                     { get; set; }

        /// <summary>
        /// Lifecycle status of the batch (Active, Depleted, Expired, Returned, WrittenOff).
        /// </summary>
        public BatchStatus Status                       { get; set; }

        /// <summary>
        /// Timestamp when the batch was created.
        /// </summary>
        public DateTime CreatedAt                       { get; set; }

        /// <summary>
        /// Timestamp when the batch was last updated.
        /// </summary>
        public DateTime? UpdatedAt                      { get; set; }

        /// <summary>
        /// User who created this batch.
        /// </summary>
        public string? CreatedBy                        { get; set; }

        /// <summary>
        /// User who last updated this batch.
        /// </summary>
        public string? UpdatedBy                        { get; set; }

        /// <summary>
        /// Indicates whether this batch is active.
        /// </summary>
        public bool IsActive                            { get; set; }
    }
}
