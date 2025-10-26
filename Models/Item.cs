using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pos_service.Models
{
    public class Item : IAuditable
    {
        /// <summary>
        /// PART 1 of the Composite Key.
        /// The main identifier or code for a product group.
        /// </summary>
        [Required]
        public int Id               { get; set; }

        /// <summary>
        /// PART 2 of the Composite Key.
        /// The sub-identifier for a variant.
        /// </summary>
        public int SubId                    { get; set; } = 0;

        /// <summary>
        /// The full name of the item for internal use (e.g., "Coca-Cola 500ml Bottle").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name                    { get; set; }

        /// <summary>
        /// The shorter name to be printed on customer receipts (e.g., "Coke 500ml").
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string PrintName               { get; set; }

        /// <summary>
        /// The EAN or UPC barcode associated with the item.
        /// </summary>
        [MaxLength(100)]
        public string? BarCode                 { get; set; }

        /// <summary>
        /// The current quantity in stock. Can be a fraction (e.g., 10.5 for kg).
        /// </summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal StockQuantity           { get; set; } = 0;

        /// <summary>
        /// If true, allows the item to be sold in fractional quantities (e.g., fruits, vegetables).
        /// This is your 'decimal' field, renamed for clarity.
        /// </summary>
        public bool AllowsDecimalQuantities   { get; set; } = false;

        // --- Pricing Information ---

        /// <summary>
        /// The cost price of the item from the supplier.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BuyingPrice            { get; set; }

        /// <summary>
        /// The marked retail price (MRP) printed on the product label.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MarkedPrice            { get; set; }

        /// <summary>
        /// The base selling price for retail customers before any discount.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RetailPrice            { get; set; }

        /// <summary>
        /// The base price for wholesale customers before any discount.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WholesalePrice         { get; set; }

        /// <summary>
        /// The discount percentage for retail sales (e.g., 5.5 for 5.5%).
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal RetailDiscountRatio    { get; set; } = 0.0m;

        /// <summary>
        /// The discount percentage for wholesale sales (e.g., 10.0 for 10%).
        /// </summary>
        [Column(TypeName = "decimal(5, 2)")]
        public decimal WholesaleDiscountRatio { get; set; } = 0.0m;

        /// <summary>
        /// A collection of suppliers that provide this item.
        /// </summary>
        public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

        // --- Implementation of IAuditable ---
        public Guid Uuid                      { get; set; }
        public DateTime CreatedAt             { get; set; }
        public DateTime? UpdatedAt            { get; set; }
        public string CreatedBy               { get; set; }
        public string? UpdatedBy              { get; set; }
        public bool IsActive                  { get; set; } = true;
    }
}
