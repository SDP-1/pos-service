using pos_service.Models.Enums;

namespace pos_service.Models.DTO.InventoryBatches
{
    /// <summary>
    /// Data Transfer Object representing an audit log snapshot of an inventory batch state change.
    /// Captures historical modifications, price changes, quantity adjustments, and user actions.
    /// </summary>
    public class InventoryBatchLogResDto
    {
        /// <summary>
        /// The unique primary key identifier of the audit log record.
        /// </summary>
        public long Id                                  { get; set; }

        /// <summary>
        /// The UUID of the audited inventory batch.
        /// </summary>
        public string BatchUuid                         { get; set; } = string.Empty;

        /// <summary>
        /// The UUID of the associated item.
        /// </summary>
        public string ItemUuid                          { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the item.
        /// </summary>
        public string? ItemName                         { get; set; }

        /// <summary>
        /// Batch number recorded at the time of logging.
        /// </summary>
        public string BatchNumber                       { get; set; } = string.Empty;

        /// <summary>
        /// Total initial quantity received into the batch.
        /// </summary>
        public decimal ReceivedQuantity                 { get; set; }

        /// <summary>
        /// Remaining quantity available at log snapshot.
        /// </summary>
        public decimal RemainingQuantity                { get; set; }

        /// <summary>
        /// Unit buying / cost price recorded in this log.
        /// </summary>
        public decimal CostPrice                        { get; set; }

        /// <summary>
        /// Marked / Maximum Retail Price recorded in this log.
        /// </summary>
        public decimal MarkedPrice                      { get; set; }

        /// <summary>
        /// Retail selling price recorded in this log.
        /// </summary>
        public decimal RetailPrice                      { get; set; }

        /// <summary>
        /// Wholesale selling price recorded in this log.
        /// </summary>
        public decimal WholesalePrice                   { get; set; }

        /// <summary>
        /// Retail discount percentage ratio.
        /// </summary>
        public decimal RetailDiscountRatio              { get; set; }

        /// <summary>
        /// Wholesale discount percentage ratio.
        /// </summary>
        public decimal WholesaleDiscountRatio           { get; set; }

        /// <summary>
        /// Invoice or reference text.
        /// </summary>
        public string? Reference                        { get; set; }

        /// <summary>
        /// The purchase record UUID, if applicable.
        /// </summary>
        public string? PurchaseUuid                     { get; set; }

        /// <summary>
        /// The supplier UUID, if applicable.
        /// </summary>
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// Name of the supplier.
        /// </summary>
        public string? SupplierName                     { get; set; }

        /// <summary>
        /// Lifecycle status of the batch.
        /// </summary>
        public BatchStatus Status                       { get; set; }

        /// <summary>
        /// Indicates whether the batch was active.
        /// </summary>
        public bool IsActive                            { get; set; }

        /// <summary>
        /// Date and time when the audit event took place.
        /// </summary>
        public DateTime ActionDate                      { get; set; }

        /// <summary>
        /// User UUID who performed the action.
        /// </summary>
        public string? ActionBy                         { get; set; }

        /// <summary>
        /// Full name of the user who performed the action.
        /// </summary>
        public string? ActionByName                     { get; set; }

        /// <summary>
        /// Trigger action type ('INSERT', 'UPDATE', etc.).
        /// </summary>
        public string Action                            { get; set; } = string.Empty; // 'INSERT', 'UPDATE'
    }
}
