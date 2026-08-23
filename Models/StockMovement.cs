using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class StockMovement
    {
        /// <summary>
        /// The unique primary key identifier for the stock movement ledger record.
        /// </summary>
        public long Id                          { get; set; }

        /// <summary>
        /// Globally unique identifier (UUID) for this movement transaction.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Uuid                      { get; set; }

        /// <summary>
        /// The UUID of the specific inventory batch affected.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string BatchUuid                 { get; set; }

        /// <summary>
        /// The UUID of the item affected by this stock movement.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ItemUuid                  { get; set; }

        /// <summary>
        /// The business trigger category of movement (Purchase, Sale, Return, Damage, Adjustment, Transfer, Count).
        /// </summary>
        public StockMovementType MovementType   { get; set; }

        /// <summary>
        /// The quantity of stock moved (always positive, direction defined by Direction property).
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity                 { get; set; }

        /// <summary>
        /// The directional effect on inventory (IN = stock added, OUT = stock removed).
        /// </summary>
        public StockMovementDirection Direction { get; set; }

        /// <summary>
        /// The unit cost price at the time of movement.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostPrice                { get; set; } = 0.0m;

        /// <summary>
        /// The source entity type creating the movement (e.g. Purchase, Order, OrderReturn, OrderDelete, ManualBatch).
        /// </summary>
        public StockMovementReferenceType? ReferenceType { get; set; }

        /// <summary>
        /// The UUID of the referencing source entity (e.g., PurchaseUuid, OrderItemUuid, AuditUuid).
        /// </summary>
        [MaxLength(255)]
        public string? ReferenceUuid            { get; set; }

        /// <summary>
        /// The standardized reason description for this movement.
        /// </summary>
        [MaxLength(500)]
        public string? Reason                   { get; set; }

        /// <summary>
        /// Optional user comments or audit notes.
        /// </summary>
        [MaxLength(500)]
        public string? Comment                  { get; set; }

        /// <summary>
        /// The timestamp when this movement was transacted and recorded.
        /// </summary>
        public DateTime CreatedAt               { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The UUID or username of the user who performed this stock movement.
        /// </summary>
        [MaxLength(255)]
        public string? CreatedBy                { get; set; }

        // Navigation properties

        /// <summary>
        /// Navigation property to the affected inventory batch entity.
        /// </summary>
        public virtual InventoryBatch? Batch    { get; set; }

        /// <summary>
        /// Navigation property to the affected item entity.
        /// </summary>
        public virtual Item? Item               { get; set; }

        /// <summary>
        /// Navigation property to the user who recorded this movement.
        /// </summary>
        public virtual User? CreatedByUser      { get; set; }
    }
}
