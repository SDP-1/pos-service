using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an inventory batch / lot in <c>tbl_inventory_batches</c>.
    /// Tracks batch numbers, received/remaining stock quantities, full-spectrum tiered pricing (cost, marked, retail, wholesale), supplier links, and lifecycle statuses.
    /// </summary>
    public class InventoryBatch : IAuditable
    {
        /// <summary>
        /// The unique primary key identifier for the inventory batch.
        /// </summary>
        public int Id                                   { get; set; }

        /// <summary>
        /// The unique identifier (UUID) of the item associated with this batch.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ItemUuid                          { get; set; }

        /// <summary>
        /// The alphanumeric batch / lot number identifier (e.g., BATCH-20260822-001).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BatchNumber                       { get; set; }

        /// <summary>
        /// The initial quantity of items received when this batch was created.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal ReceivedQuantity                 { get; set; }

        /// <summary>
        /// The current remaining quantity of items available in this batch.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal RemainingQuantity                { get; set; }

        // ═══ FULL BATCH-LEVEL PRICING ═══

        /// <summary>
        /// The unit cost / purchase buying price paid for items in this batch.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostPrice                        { get; set; } = 0.0m;

        /// <summary>
        /// The manufacturer marked / maximum retail price (MRP) printed on items in this batch.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPrice                      { get; set; } = 0.0m;

        /// <summary>
        /// The final selling price for standard retail customers for this batch.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RetailPrice                      { get; set; } = 0.0m;

        /// <summary>
        /// The discounted selling price for wholesale / bulk buyers for this batch.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WholesalePrice                   { get; set; } = 0.0m;

        /// <summary>
        /// The retail discount percentage ratio calculated from marked price.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal RetailDiscountRatio              { get; set; } = 0.0m;

        /// <summary>
        /// The wholesale discount percentage ratio calculated from marked price.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal WholesaleDiscountRatio           { get; set; } = 0.0m;

        // ═══ REFERENCES ═══

        /// <summary>
        /// External invoice or GRN reference note associated with this batch.
        /// </summary>
        [MaxLength(200)]
        public string? Reference                        { get; set; }

        /// <summary>
        /// The UUID of the purchase record from which this batch originated, if applicable.
        /// </summary>
        [MaxLength(255)]
        public string? PurchaseUuid                     { get; set; }

        /// <summary>
        /// The UUID of the supplier who provided items in this batch, if applicable.
        /// </summary>
        [MaxLength(255)]
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// The active lifecycle status of the batch (Active, Depleted, Expired, Returned, WrittenOff).
        /// </summary>
        public BatchStatus Status                       { get; set; } = BatchStatus.Active;

        // Navigation properties

        /// <summary>
        /// Navigation property to the parent item entity.
        /// </summary>
        public virtual Item? Item                       { get; set; }

        /// <summary>
        /// Navigation property to the purchase order entity.
        /// </summary>
        public virtual Purchase? Purchase               { get; set; }

        /// <summary>
        /// Navigation property to the supplier entity.
        /// </summary>
        public virtual Supplier? Supplier               { get; set; }

        /// <summary>
        /// Collection of stock movement ledger records associated with this batch.
        /// </summary>
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

        // ═══ IAuditable ═══

        /// <summary>
        /// Globally unique identifier (UUID) for this batch.
        /// </summary>
        public string Uuid                              { get; set; }

        /// <summary>
        /// Timestamp when the batch record was created.
        /// </summary>
        public DateTime CreatedAt                       { get; set; }

        /// <summary>
        /// Timestamp when the batch record was last updated.
        /// </summary>
        public DateTime? UpdatedAt                      { get; set; }

        /// <summary>
        /// Identifier or username of the user who created this batch.
        /// </summary>
        public string CreatedBy                         { get; set; }

        /// <summary>
        /// Identifier or username of the user who last updated this batch.
        /// </summary>
        public string? UpdatedBy                        { get; set; }

        /// <summary>
        /// Indicates whether this batch record is active (soft delete flag).
        /// </summary>
        public bool IsActive                            { get; set; } = true;
    }
}
