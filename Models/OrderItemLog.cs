using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an immutable log entry in <c>tbl_order_item_logs</c>.
    /// Automatically captured via database triggers on order item INSERT, UPDATE, and DELETE actions.
    /// Aligned 1-to-1 with <c>tbl_order_items</c>.
    /// </summary>
    public class OrderItemLog
    {
        /// <summary>
        /// The unique primary key identifier for this audit log entry.
        /// </summary>
        [Key]
        [Column("LogId")]
        public long Id { get; set; }

        /// <summary>
        /// The primary key Id of the source order item record in <c>tbl_order_items</c>.
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// The foreign key identifier of the parent sales order in <c>tbl_orders</c>.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// The UUID of the original catalog item associated with this line item.
        /// </summary>
        [MaxLength(36)]
        public string? OriginalItemUuid { get; set; }

        /// <summary>
        /// The UUID of the inventory batch allocated for this line item.
        /// </summary>
        [MaxLength(36)]
        public string? BatchUuid { get; set; }

        /// <summary>
        /// If true, allows the item to be sold in fractional or decimal quantities (e.g., kg, grams).
        /// </summary>
        public bool AllowsDecimalQuantities { get; set; } = false;

        /// <summary>
        /// The snapshotted receipt print name of the item at the moment of sale.
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string PrintName { get; set; }

        /// <summary>
        /// The quantity sold or returned in this line item.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The snapshotted unit selling price at the moment of sale.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceAtSale { get; set; }

        /// <summary>
        /// The snapshotted manufacturer marked price (MRP) at the moment of sale.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPriceAtSale { get; set; } = 0.0m;

        /// <summary>
        /// The snapshotted unit cost price of the item from its batch at the moment of sale.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostAtSale { get; set; }

        /// <summary>
        /// The total gross line amount (Quantity * PriceAtSale) for this line item.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Indicates whether this order item represents a customer return transaction.
        /// </summary>
        public bool IsReturnItem { get; set; } = false;

        /// <summary>
        /// The UUID of the original sold order item being returned, if this line is a return.
        /// </summary>
        [MaxLength(36)]
        public string? ReturnedOrderItemUuid { get; set; }

        /// <summary>
        /// Optional line item notes, specifications, or custom descriptions.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// The business UUID of the source order item entity (<c>tbl_order_items.Uuid</c>).
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string OrderItemUuid { get; set; }

        /// <summary>
        /// Date and time when the original order item was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the original order item was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// The UUID of the user who originally created this order item.
        /// </summary>
        [MaxLength(36)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UUID of the user who last updated this order item.
        /// </summary>
        [MaxLength(36)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indicates whether the original order item was active (1) or soft-deleted (0).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The trigger action type that produced this log entry (e.g., A.INSERT, A.UPDATE, A.DELETE).
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Action { get; set; }

        /// <summary>
        /// The exact UTC timestamp when this audit action was executed.
        /// </summary>
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// The UUID of the user who performed the operation triggering this audit log.
        /// </summary>
        [MaxLength(36)]
        public string? ActionBy { get; set; }

        /// <summary>
        /// Navigation property to the User entity who performed the logged action.
        /// </summary>
        public virtual User? ActionByUser { get; set; }
    }
}
