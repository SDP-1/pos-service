using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using pos_service.Models.Audit;

namespace pos_service.Models
{
    /// <summary>
    /// Entity representing an item expiration date record in <c>tbl_item_expiries</c>.
    /// Stores expiration dates and notification threshold days for near-expiry and expired item alerts.
    /// </summary>
    [Table("tbl_item_expiries")]
    public class ItemExpiry : IAuditable
    {
        /// <summary>
        /// The unique primary key identifier for the item expiry record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The main ID of the associated item.
        /// </summary>
        public int ItemsId { get; set; }

        /// <summary>
        /// The sub-variant ID of the associated item.
        /// </summary>
        public int ItemsSubId { get; set; }

        /// <summary>
        /// The unique identifier (UUID) of the item.
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string ItemUuid { get; set; } = string.Empty;

        /// <summary>
        /// The expiration date of the product stock.
        /// </summary>
        [Required]
        public DateTime ExpDate { get; set; }

        /// <summary>
        /// Number of days before the expiration date to begin triggering warning notifications.
        /// </summary>
        public int NotifyBeforeDays { get; set; } = 0;

        /// <summary>
        /// Globally unique identifier (UUID) for this expiry record.
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string Uuid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp when this expiry record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when this expiry record was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Identifier or username of the user who created this expiry record.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Identifier or username of the user who last updated this expiry record.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indicates whether this expiry record is active (soft delete flag).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property to the associated item entity.
        /// </summary>
        [ForeignKey("ItemUuid")]
        public virtual Item? Item { get; set; }
    }
}
