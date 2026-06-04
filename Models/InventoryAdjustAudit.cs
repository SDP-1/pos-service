using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    /// <summary>
    /// Audit record for inventory adjustments. Automatically logged by database triggers
    /// when inventory stock is adjusted.
    /// </summary>
    public class InventoryAdjustAudit : IAuditable
    {
        public int Id                      { get; set; }

        [Required]
        [MaxLength(255)]
        public string InventoryUuid        { get; set; }

        [Required]
        [MaxLength(255)]
        public string ItemUuid             { get; set; }

        /// <summary>
        /// Previous stock quantity before adjustment.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal PreviousQuantity    { get; set; }

        /// <summary>
        /// New stock quantity after adjustment.
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal NewQuantity         { get; set; }

        /// <summary>
        /// Quantity that was added or removed (can be positive or negative).
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal AdjustmentQuantity  { get; set; }

        /// <summary>
        /// Unit type of the adjustment.
        /// </summary>
        public UnitType UnitType           { get; set; }

        /// <summary>
        /// True if quantity was increased, false if decreased.
        /// </summary>
        public bool Increase               { get; set; }

        /// <summary>
        /// Optional comment about the adjustment.
        /// </summary>
        [MaxLength(500)]
        public string? Comment             { get; set; }

        /// <summary>
        /// Reason for the adjustment (especially for stock decreases).
        /// </summary>
        [MaxLength(500)]
        public string? Reason              { get; set; }

        public virtual Inventory Inventory { get; set; }

        // Audit properties
        public string Uuid                 { get; set; }
        public DateTime CreatedAt          { get; set; }
        public DateTime? UpdatedAt         { get; set; }
        public string CreatedBy            { get; set; }
        public string? UpdatedBy           { get; set; }
        public bool IsActive               { get; set; } = true;
    }
}
