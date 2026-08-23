using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an individual line item record within a sales order in <c>tbl_order_items</c>.
    /// Preserves immutable point-of-sale snapshots for print name, sale price, marked price, unit cost, quantity, batch link, and returns.
    /// </summary>
    public class OrderItem : IAuditable
    {
        /// <summary>
        /// The unique primary key identifier for the order item record.
        /// </summary>
        public int Id                       { get; set; }

        // --- Link to the parent Order ---

        /// <summary>
        /// The foreign key identifier linking this line item to its parent order.
        /// </summary>
        public int OrderId                  { get; set; }

        /// <summary>
        /// Navigation property to the parent sales order.
        /// </summary>
        public virtual Order Order          { get; set; }

        // --- Optional link to the original Item for historical analysis ---

        /// <summary>
        /// The UUID of the original catalog item sold in this line.
        /// </summary>
        public string? OriginalItemUuid     { get; set; }

        /// <summary>
        /// Navigation property to the original catalog item.
        /// </summary>
        public virtual Item? Item           { get; set; }

        // --- Link to the specific Inventory Batch consumed ---

        /// <summary>
        /// The UUID of the specific inventory batch allocated for this line item.
        /// </summary>
        [MaxLength(255)]
        public string? BatchUuid            { get; set; }

        /// <summary>
        /// Navigation property to the inventory batch entity.
        /// </summary>
        public virtual InventoryBatch? Batch { get; set; }

        /// <summary>
        /// If true, allows the item to be sold in fractional quantities (e.g., fruits, vegetables).
        /// </summary>
        public bool AllowsDecimalQuantities { get; set; } = false;

        // --- SNAPSHOTTED DATA ---
        // These fields are copied from the Item table at the time of sale.

        /// <summary>
        /// The snapshotted name of the item as it appeared on the printed receipt at the moment of sale.
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string PrintName             { get; set; }

        /// <summary>
        /// The quantity sold (can be fractional for weighted goods).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity             { get; set; }

        /// <summary>
        /// The snapshotted unit sale price at the moment of sale.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceAtSale          { get; set; }

        /// <summary>
        /// The snapshotted marked price (MRP) of the item at the moment of sale.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPriceAtSale  { get; set; }

        /// <summary>
        /// The snapshotted unit cost of the item at the moment of sale (used for profit calculations).
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostAtSale           { get; set; }

        /// <summary>
        /// The final line total price for this item (Quantity * PriceAtSale minus line discount).
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LineTotal            { get; set; }

        /// <summary>
        /// Indicates if this order item is a return/refund.
        /// When true, the quantity was added back to the original item instead of being deducted.
        /// </summary>
        public bool IsReturnItem            { get; set; } = false;

        /// <summary>
        /// The UUID of the returned OrderItem (reference to the original line being returned).
        /// Used to track returns against the specific original OrderItem.
        /// </summary>
        [MaxLength(36)]
        public string? ReturnedOrderItemUuid { get; set; }

        /// <summary>
        /// Optional description for this item line (custom notes or return remarks).
        /// </summary>
        [MaxLength(500)]
        public string? Description          { get; set; }

        // --- Implementation of IAuditable ---

        /// <summary>
        /// Globally unique identifier (UUID) for this order item record.
        /// </summary>
        public string Uuid                  { get; set; }

        /// <summary>
        /// Timestamp when this line item was created.
        /// </summary>
        public DateTime CreatedAt           { get; set; }

        /// <summary>
        /// Timestamp when this line item was last updated.
        /// </summary>
        public DateTime? UpdatedAt          { get; set; }

        /// <summary>
        /// Identifier or username of the user who recorded this line item.
        /// </summary>
        public string CreatedBy             { get; set; }

        /// <summary>
        /// Identifier or username of the user who last updated this line item.
        /// </summary>
        public string? UpdatedBy            { get; set; }

        /// <summary>
        /// Indicates whether this order item record is active.
        /// </summary>
        public bool IsActive                { get; set; } = true;
    }
}
