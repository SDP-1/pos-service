using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Inventory
{
    /// <summary>
    /// Request DTO for querying inventory adjustment audit history.
    /// All parameters except ItemUuid are optional.
    /// </summary>
    public class InventoryAuditHistoryReqDto
    {
        /// <summary>
        /// Item UUID to retrieve audit history for. REQUIRED.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ItemUuid     { get; set; }

        /// <summary>
        /// Start date for filtering adjustments (optional).
        /// If null, no start date filter is applied.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for filtering adjustments (optional).
        /// If null, no end date filter is applied.
        /// </summary>
        public DateTime? EndDate   { get; set; }

        /// <summary>
        /// Maximum number of records to return (optional).
        /// If null, defaults to 100.
        /// </summary>
        [Range(1, 10000)]
        public int? MaxRecords     { get; set; }
    }
}
