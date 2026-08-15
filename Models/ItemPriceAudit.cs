using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class ItemPriceAudit
    {
        public long Id                          { get; set; }

        public int ItemsId                      { get; set; }
        public int ItemsSubId                   { get; set; }

        public string ItemUuid                  { get; set; } = string.Empty;

        /// <summary>
        /// Historical buying/cost price of the item from the supplier.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BuyingPrice              { get; set; }

        /// <summary>
        /// Historical marked retail price (MRP).
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPrice              { get; set; }

        /// <summary>
        /// Historical base selling price for retail customers.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RetailPrice              { get; set; }

        /// <summary>
        /// Historical base price for wholesale customers.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WholesalePrice           { get; set; }

        /// <summary>
        /// Historical discount percentage for retail sales.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal RetailDiscountRatio      { get; set; } = 0.0m;

        /// <summary>
        /// Historical discount percentage for wholesale sales.
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal WholesaleDiscountRatio   { get; set; } = 0.0m;

        /// <summary>
        /// Timestamp when price change occurred.
        /// </summary>
        public DateTime ChangedAt               { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User or system that executed the price change.
        /// </summary>
        public string? ChangedBy                { get; set; }

        /// <summary>
        /// Change type ('INSERT' or 'UPDATE').
        /// </summary>
        public string ChangeType                { get; set; }
    }
}
