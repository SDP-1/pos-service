using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    /// <summary>
    /// Request Data Transfer Object for creating or updating an item / product.
    /// In accordance with domain design, AllowsDecimalQuantities is a direct property of the Item.
    /// </summary>
    public class ItemReqDto : IReqAuditDto
    {
        /// <summary>
        /// Optional main Item ID. If null or 0 on creation, the service auto-assigns the next available main ID.
        /// </summary>
        public int? Id                        { get; set; }

        /// <summary>
        /// Optional sub-variant ID. If null, the service auto-assigns the next sub ID for this item family.
        /// </summary>
        public int? SubId                     { get; set; }

        // Backing fields to normalize values on set
        private string _name;
        private string _printName;

        /// <summary>
        /// Full descriptive name of the item / product (auto-uppercased).
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name
        {
            get => _name;
            set => _name = value?.ToUpperInvariant();
        }

        /// <summary>
        /// Shortened receipt print name (maximum 40 chars, auto-uppercased).
        /// </summary>
        [Required]
        [StringLength(40)]
        public string PrintName
        {
            get => _printName;
            set => _printName = value?.ToUpperInvariant();
        }

        /// <summary>
        /// Barcode string for scanner lookup.
        /// </summary>
        [StringLength(100)]
        public string? BarCode                { get; set; }

        /// <summary>
        /// Extended description or specifications for the item.
        /// </summary>
        [StringLength(500)]
        public string? Description            { get; set; }

        /// <summary>
        /// Indicates whether this item allows fractional or decimal quantities (e.g., kg, grams, meters).
        /// This is a direct column in tbl_items.
        /// </summary>
        public bool AllowsDecimalQuantities   { get; set; } = false;

        /// <summary>
        /// Initial opening stock quantity for the first batch when creating an item.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal StockQuantity          { get; set; } = 0;

        /// <summary>
        /// Base unit type for this item (e.g. Kilogram, Piece, Packet).
        /// </summary>
        public UnitType UnitType              { get; set; } = UnitType.None;

        /// <summary>
        /// Packaging units hierarchy configured for this item.
        /// </summary>
        public List<InventoryUnitReqDto> Units { get; set; } = new();

        /// <summary>
        /// Optional inventory wrapper object from clients.
        /// </summary>
        public InventoryReqDto? Inventory     { get; set; }

        /// <summary>
        /// Full pricing tier definitions (buying/cost price, marked price, retail, wholesale, discounts).
        /// </summary>
        [Required]
        public ItemPriceReqDto Price          { get; set; }

        /// <summary>
        /// Expiration date records and threshold notification configurations.
        /// </summary>
        public ICollection<ItemExpiryReqDto> ExpDates { get; set; } = new List<ItemExpiryReqDto>();

        /// <summary>
        /// A list of supplier IDs to associate with this item.
        /// </summary>
        public ICollection<int> SupplierIds   { get; set; } = new List<int>();

        /// <summary>
        /// Indicates whether this item record is active.
        /// </summary>
        public bool IsActive                  { get; set; } = true;
    }
}
