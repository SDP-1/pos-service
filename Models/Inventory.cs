using pos_service.Models.Audit;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class Inventory : IAuditable
    {
        public int Id                                   { get; set; }

        [Required]
        [MaxLength(255)]
        public string ItemUuid                          { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal StockQuantity                    { get; set; } = 0;

        public bool AllowsDecimalQuantities             { get; set; } = false;

        public UnitType UnitType                        { get; set; } = UnitType.Each;

        /// <summary>
        /// Optional comment about the current inventory state or last adjustment.
        /// </summary>
        [MaxLength(500)]
        public string? Comment                          { get; set; }

        /// <summary>
        /// Reason for the last inventory adjustment (required if decrease and setting is enabled).
        /// </summary>
        [MaxLength(500)]
        public string? Reason                           { get; set; }

        /// <summary>
        /// Indicates whether this inventory has been manually adjusted by a user.
        /// True when adjusted via InventoryService.AdjustStockAsync or item edit.
        /// False for initial creation or automatic operations like order processing.
        /// </summary>
        public bool IsUserAdjusted                      { get; set; } = false;

        public virtual Item Item                        { get; set; }

        public virtual ICollection<InventoryUnit> Units { get; set; } = new List<InventoryUnit>();

        public string Uuid                              { get; set; }
        public DateTime CreatedAt                       { get; set; }
        public DateTime? UpdatedAt                      { get; set; }
        public string CreatedBy                         { get; set; }
        public string? UpdatedBy                        { get; set; }
        public bool IsActive                            { get; set; } = true;
    }
}
