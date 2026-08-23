using pos_service.Models.DTO.InventoryBatches;
using pos_service.Models.DTO.Items;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Purchases
{
    /// <summary>
    /// Request DTO representing a line item entry within a purchase receipt.
    /// </summary>
    public class PurchaseItemReqDto
    {
        /// <summary>
        /// Unique UUID of the item being received.
        /// </summary>
        [Required]
        public string ItemUuid                          { get; set; } = string.Empty;

        /// <summary>
        /// Optional custom batch lot number. If not provided, an auto-generated batch number is assigned.
        /// </summary>
        public string? BatchNumber                      { get; set; }

        /// <summary>
        /// Received quantity of the item in base inventory units.
        /// </summary>
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity                         { get; set; }

        /// <summary>
        /// Unit procurement cost price paid to the supplier.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Cost price must be non-negative")]
        public decimal CostPrice                        { get; set; } = 0.0m;

        /// <summary>
        /// Maximum retail or marked selling price (MRP).
        /// </summary>
        public decimal MarkedPrice                      { get; set; } = 0.0m;

        /// <summary>
        /// Retail selling price tier for consumer point-of-sale transactions.
        /// </summary>
        public decimal RetailPrice                      { get; set; } = 0.0m;

        /// <summary>
        /// Wholesale selling price tier for bulk trade transactions.
        /// </summary>
        public decimal WholesalePrice                   { get; set; } = 0.0m;

        /// <summary>
        /// Percentage discount ratio applied against Marked Price for Retail sales.
        /// </summary>
        public decimal RetailDiscountRatio              { get; set; } = 0.0m;

        /// <summary>
        /// Percentage discount ratio applied against Marked Price for Wholesale sales.
        /// </summary>
        public decimal WholesaleDiscountRatio           { get; set; } = 0.0m;

        /// <summary>
        /// Optional external reference, shipment tag, or PO note for this line item.
        /// </summary>
        public string? Reference                        { get; set; }

        /// <summary>
        /// Optional expiry dates and notification thresholds recorded for this stock batch.
        /// </summary>
        public List<ItemExpiryReqDto>? ExpDates         { get; set; }
    }

    /// <summary>
    /// Request DTO for creating a new purchase receipt and receiving stock batches.
    /// </summary>
    public class PurchaseReqDto
    {
        /// <summary>
        /// Optional unique UUID of the supplier supplying the items.
        /// </summary>
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// Optional invoice or bill number issued by the supplier.
        /// </summary>
        public string? InvoiceNumber                    { get; set; }

        /// <summary>
        /// Date when the purchase was executed or goods were received.
        /// </summary>
        public DateTime PurchaseDate                    { get; set; } = DateTime.UtcNow.Date;

        /// <summary>
        /// Optional remarks or notes attached to the purchase receipt.
        /// </summary>
        public string? Notes                            { get; set; }

        /// <summary>
        /// List of purchase line items included in the receipt.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one item must be included in the purchase")]
        public List<PurchaseItemReqDto> Items          { get; set; } = new();
    }

    /// <summary>
    /// Response DTO representing a saved purchase receipt with associated stock batches.
    /// </summary>
    public class PurchaseResDto
    {
        /// <summary>
        /// Database auto-increment identifier.
        /// </summary>
        public int Id                                   { get; set; }

        /// <summary>
        /// Unique UUID identifier of the purchase receipt.
        /// </summary>
        public string Uuid                              { get; set; } = string.Empty;

        /// <summary>
        /// Unique human-readable purchase receipt tracking number (e.g., PUR-20260822-0001).
        /// </summary>
        public string PurchaseNumber                    { get; set; } = string.Empty;

        /// <summary>
        /// Unique UUID of the supplier.
        /// </summary>
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// Resolved name of the supplier.
        /// </summary>
        public string? SupplierName                     { get; set; }

        /// <summary>
        /// Supplier's invoice or bill number.
        /// </summary>
        public string? InvoiceNumber                    { get; set; }

        /// <summary>
        /// Execution or receipt date of the purchase.
        /// </summary>
        public DateTime PurchaseDate                    { get; set; }

        /// <summary>
        /// Aggregate procurement cost of all line items in the purchase.
        /// </summary>
        public decimal TotalCost                        { get; set; }

        /// <summary>
        /// Total number of distinct line items in the purchase receipt.
        /// </summary>
        public int TotalItems                           { get; set; }

        /// <summary>
        /// Current lifecycle status of the purchase receipt.
        /// </summary>
        public PurchaseStatus Status                    { get; set; }

        /// <summary>
        /// Optional remarks or notes attached to the purchase receipt.
        /// </summary>
        public string? Notes                            { get; set; }

        /// <summary>
        /// Creation timestamp in UTC.
        /// </summary>
        public DateTime CreatedAt                       { get; set; }

        /// <summary>
        /// User UUID of the creator.
        /// </summary>
        public string? CreatedBy                        { get; set; }

        /// <summary>
        /// Full name of the creator user.
        /// </summary>
        public string? CreatedByName                    { get; set; }

        /// <summary>
        /// Last modification timestamp in UTC.
        /// </summary>
        public DateTime? UpdatedAt                      { get; set; }

        /// <summary>
        /// User UUID of the last modifier.
        /// </summary>
        public string? UpdatedBy                        { get; set; }

        /// <summary>
        /// Soft deletion / active status flag.
        /// </summary>
        public bool IsActive                            { get; set; }

        /// <summary>
        /// Inventory batch lots created and associated with this purchase receipt.
        /// </summary>
        public List<InventoryBatchResDto> Batches       { get; set; } = new();
    }
}
