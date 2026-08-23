using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class Purchase : IAuditable
    {
        /// <summary>
        /// The unique primary key identifier for the purchase record.
        /// </summary>
        public int Id                                       { get; set; }

        /// <summary>
        /// User-friendly purchase identifier number (e.g. PO-20260822-001).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PurchaseNumber                        { get; set; }

        /// <summary>
        /// The UUID of the supplier from whom the purchase was made.
        /// </summary>
        [MaxLength(255)]
        public string? SupplierUuid                         { get; set; }

        /// <summary>
        /// Supplier's external invoice or delivery note reference number.
        /// </summary>
        [MaxLength(100)]
        public string? InvoiceNumber                        { get; set; }

        /// <summary>
        /// The transaction / goods receipt date of the purchase.
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime PurchaseDate                        { get; set; } = DateTime.UtcNow.Date;

        /// <summary>
        /// The total purchasing cost for all items received in this order.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost                            { get; set; } = 0.0m;

        /// <summary>
        /// Total distinct line item count included in this purchase.
        /// </summary>
        public int TotalItems                               { get; set; } = 0;

        /// <summary>
        /// The fulfillment / return status of the purchase order.
        /// </summary>
        public PurchaseStatus Status                        { get; set; } = PurchaseStatus.Received;

        /// <summary>
        /// Optional purchase remarks, delivery notes, or terms.
        /// </summary>
        [MaxLength(500)]
        public string? Notes                                { get; set; }

        // Navigation

        /// <summary>
        /// Navigation property to the supplying vendor entity.
        /// </summary>
        public virtual Supplier? Supplier                   { get; set; }

        /// <summary>
        /// Collection of inventory batches created from this purchase.
        /// </summary>
        public virtual ICollection<InventoryBatch> Batches  { get; set; } = new List<InventoryBatch>();

        // IAuditable

        /// <summary>
        /// Globally unique identifier (UUID) for this purchase.
        /// </summary>
        public string Uuid                                  { get; set; }

        /// <summary>
        /// Timestamp when the purchase record was created.
        /// </summary>
        public DateTime CreatedAt                           { get; set; }

        /// <summary>
        /// Timestamp when the purchase record was last modified.
        /// </summary>
        public DateTime? UpdatedAt                          { get; set; }

        /// <summary>
        /// Identifier or username of the user who recorded this purchase.
        /// </summary>
        public string CreatedBy                             { get; set; }

        /// <summary>
        /// Identifier or username of the user who last updated this purchase.
        /// </summary>
        public string? UpdatedBy                            { get; set; }

        /// <summary>
        /// Indicates whether this purchase record is active (soft delete flag).
        /// </summary>
        public bool IsActive                                { get; set; } = true;
    }
}
