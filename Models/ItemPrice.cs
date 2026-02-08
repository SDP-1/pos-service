using pos_service.Models.Audit;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class ItemPrice : IAuditable
    {
        public int ItemsId { get; set; }
        public int ItemsSubId { get; set; }

        public string ItemUuid { get; set; }

        /// <summary>
        /// The cost price of the item from the supplier.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BuyingPrice { get; set; }

        /// <summary>
        /// The marked retail price (MRP) printed on the product label.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPrice { get; set; }

        /// <summary>
        /// The base selling price for retail customers before any discount.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RetailPrice { get; set; }

        /// <summary>
        /// The base price for wholesale customers before any discount.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WholesalePrice { get; set; }

        /// <summary>
        /// The discount percentage for retail sales (e.g., 5.5 for 5.5%).
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal RetailDiscountRatio { get; set; } = 0.0m;

        /// <summary>
        /// The discount percentage for wholesale sales (e.g., 10.0 for 10%).
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal WholesaleDiscountRatio { get; set; } = 0.0m;

        public virtual Item Item { get; set; }

        // --- Implementation of IAuditable ---
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
