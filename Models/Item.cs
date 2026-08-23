using pos_service.Models.Audit;
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
        public int Id                          { get; set; }

        /// <summary>
        /// PART 2 of the Composite Key.
        /// The sub-identifier for a variant.
        /// </summary>
        [Required]
        public int SubId                       { get; set; } = 0;

        /// <summary>
        /// The full name of the item for internal use (e.g., "Coca-Cola 500ml Bottle").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name                     { get; set; }

        /// <summary>
        /// The shorter name to be printed on customer receipts (e.g., "Coke 500ml").
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string PrintName                { get; set; }

        /// <summary>
        /// The EAN or UPC barcode associated with the item.
        /// </summary>
        [MaxLength(100)]
        public string? BarCode                 { get; set; }

        /// <summary>
        /// Optional description or notes for the item.
        /// </summary>
        [MaxLength(500)]
        public string? Description             { get; set; }


        /// <summary>
        /// Expiry dates for this item.
        /// </summary>
        public virtual ICollection<ItemExpiry> ExpDates { get; set; } = new List<ItemExpiry>();

        /// <summary>
        /// Junction entities linking this item to suppliers.
        /// Use the `ItemSupplier` entity when you need additional columns on the relationship.
        /// </summary>
        public virtual ICollection<ItemSupplier> ItemSuppliers { get; set; } = new List<ItemSupplier>();

        /// <summary>
        /// Whether this item allows fractional / decimal quantities.
        /// </summary>
        public bool AllowsDecimalQuantities    { get; set; } = false;

        /// <summary>
        /// Packaging units (Pack, Box, Carton) linked directly to this item.
        /// </summary>
        public virtual ICollection<ItemUnit> Units { get; set; } = new List<ItemUnit>();


        /// <summary>
        /// Inventory batches associated with this item.
        /// </summary>
        public virtual ICollection<InventoryBatch> Batches { get; set; } = new List<InventoryBatch>();

        // --- Implementation of IAuditable ---
        public string Uuid                    { get; set; }
        public DateTime CreatedAt             { get; set; }
        public DateTime? UpdatedAt            { get; set; }
        public string CreatedBy               { get; set; }
        public string? UpdatedBy              { get; set; }
        public bool IsActive                  { get; set; } = true;
    }
}
