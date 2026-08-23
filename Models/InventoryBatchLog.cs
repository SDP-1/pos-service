using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an immutable audit log row in <c>tbl_inventory_batch_logs</c>.
    /// Automatically records insert, update, price adjustment, or status modification history for inventory batches.
    /// </summary>
    public class InventoryBatchLog
    {
        /// <summary>
        /// The primary key identifier for this audit log entry.
        /// </summary>
        [Key]
        [Column("LogId")]
        public long Id                                  { get; set; }

        /// <summary>
        /// The primary key Id of the source batch.
        /// </summary>
        public int? BatchId                             { get; set; }

        /// <summary>
        /// The UUID of the batch being audited.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string BatchUuid                         { get; set; }

        /// <summary>
        /// The UUID of the item associated with the audited batch.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ItemUuid                          { get; set; }

        /// <summary>
        /// The batch number at the time of the log snapshot.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BatchNumber                       { get; set; }

        /// <summary>
        /// The initial received quantity recorded in the batch.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal ReceivedQuantity                 { get; set; }

        /// <summary>
        /// The remaining stock quantity at the time of logging.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal RemainingQuantity                { get; set; }

        /// <summary>
        /// The unit cost price recorded in this log snapshot.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostPrice                        { get; set; } = 0.0m;

        /// <summary>
        /// The manufacturer marked price recorded in this log snapshot.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPrice                      { get; set; } = 0.0m;

        /// <summary>
        /// The retail selling price recorded in this log snapshot.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RetailPrice                      { get; set; } = 0.0m;

        /// <summary>
        /// The wholesale selling price recorded in this log snapshot.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WholesalePrice                   { get; set; } = 0.0m;

        /// <summary>
        /// The retail discount ratio recorded in this log snapshot.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal RetailDiscountRatio              { get; set; } = 0.0m;

        /// <summary>
        /// The wholesale discount ratio recorded in this log snapshot.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal WholesaleDiscountRatio           { get; set; } = 0.0m;

        /// <summary>
        /// The invoice or reference note at the time of logging.
        /// </summary>
        [MaxLength(200)]
        public string? Reference                        { get; set; }

        /// <summary>
        /// The purchase record UUID at the time of logging.
        /// </summary>
        [MaxLength(255)]
        public string? PurchaseUuid                     { get; set; }

        /// <summary>
        /// The supplier UUID at the time of logging.
        /// </summary>
        [MaxLength(255)]
        public string? SupplierUuid                     { get; set; }

        /// <summary>
        /// The lifecycle status of the batch at the time of logging.
        /// </summary>
        public BatchStatus Status                       { get; set; } = BatchStatus.Active;

        /// <summary>
        /// Original batch creation timestamp.
        /// </summary>
        public DateTime CreatedAt                       { get; set; }

        /// <summary>
        /// Username or identifier of the original creator.
        /// </summary>
        public string? CreatedBy                        { get; set; }

        /// <summary>
        /// Batch last update timestamp prior to this action.
        /// </summary>
        public DateTime? UpdatedAt                      { get; set; }

        /// <summary>
        /// Username or identifier of the user who last updated the batch.
        /// </summary>
        public string? UpdatedBy                        { get; set; }

        /// <summary>
        /// Indicates if the batch was active at the time of logging.
        /// </summary>
        public bool IsActive                            { get; set; } = true;

        /// <summary>
        /// The UTC timestamp when this audit log entry was generated.
        /// </summary>
        public DateTime ActionDate                      { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The user UUID who performed the triggering action.
        /// </summary>
        [MaxLength(255)]
        public string? ActionBy                         { get; set; }

        /// <summary>
        /// The type of database trigger action performed ('INSERT', 'UPDATE', 'DELETE').
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Action                            { get; set; } // 'INSERT', 'UPDATE'

        // Navigation

        /// <summary>
        /// Navigation property to the item entity.
        /// </summary>
        public virtual Item? Item                       { get; set; }

        /// <summary>
        /// Navigation property to the audited batch entity.
        /// </summary>
        public virtual InventoryBatch? Batch            { get; set; }

        /// <summary>
        /// Navigation property to the user who triggered the action.
        /// </summary>
        public virtual User? ActionByUser               { get; set; }
    }
}
