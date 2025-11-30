using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class OrderItem : IAuditable
    {
        public int Id                       { get; set; }

        // --- Link to the parent Order ---
        public int OrderId                  { get; set; }
        public virtual Order Order          { get; set; }

        // --- Optional link to the original Item for historical analysis ---
        public string? OriginalItemUuid     { get; set; }
        public virtual Item? Item           { get; set; }

        /// <summary>
        /// If true, allows the item to be sold in fractional quantities (e.g., fruits, vegetables).
        /// This is your 'decimal' field, renamed for clarity.
        /// </summary>
        public bool AllowsDecimalQuantities { get; set; } = false;

        // --- SNAPSHOTTED DATA ---
        // These fields are copied from the Item table at the time of sale.

        /// <summary>
        /// The name of the item as it appeared on the receipt.
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string ItemPrintName         { get; set; }

        /// <summary>
        /// The quantity sold (can be fractional).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Quantity             { get; set; }

        /// <summary>
        /// The unit price of the item at the moment of sale.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceAtSale          { get; set; }

        /// <summary>
        /// The discount ratio of the item at the moment of sale.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountRatioAtSale  { get; set; }

        /// <summary>
        /// The cost of the item at the moment of sale.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CostAtSale           { get; set; }

        /// <summary>
        /// The total price for this line item after discount (Quantity * Price * (1 - DiscountRatio/100)).
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LineTotal            { get; set; }

        // --- Implementation of IAuditable ---
        public string Uuid                  { get; set; }
        public DateTime CreatedAt           { get; set; }
        public DateTime? UpdatedAt          { get; set; }
        public string CreatedBy             { get; set; }
        public string? UpdatedBy            { get; set; }
        public bool IsActive                { get; set; } = true;
    }
}
