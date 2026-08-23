using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    /// <summary>
    /// Request Data Transfer Object for creating or updating an item / product.
    /// Encapsulates product names, barcode, pricing structures, unit types, expiry tracking, packaging levels, and initial inventory parameters.
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
        /// Initial stock quantity to record for this item.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal StockQuantity          { get; set; } = 0;

        /// <summary>
        /// If true, allows sale and stock tracking in fractional / decimal quantities (e.g. kg, grams).
        /// </summary>
        public bool AllowsDecimalQuantities   { get; set; } = false;

        private UnitType? _unitType;

        /// <summary>
        /// The base unit type for the item (e.g., Kilogram, Packet, Item).
        /// </summary>
        public UnitType UnitType
        {
            get
            {
                if (_unitType.HasValue && _unitType != UnitType.None)
                    return _unitType.Value;

                // Dynamic default logic
                return AllowsDecimalQuantities
                    ? UnitType.Kilogram
                    : UnitType.Packet;
            }
            set => _unitType = value;
        }

        /// <summary>
        /// Multi-level packaging unit definitions configured for this item.
        /// </summary>
        public List<InventoryUnitReqDto> Units        { get; set; } = new();

        /// <summary>
        /// Full pricing tier definitions (buying/cost price, marked price, retail, wholesale, discounts).
        /// </summary>
        [Required]
        public ItemPriceReqDto Price                  { get; set; }

        /// <summary>
        /// Expiration date records and threshold notification configurations.
        /// </summary>
        public ICollection<ItemExpiryReqDto> ExpDates { get; set; } = new List<ItemExpiryReqDto>();

        /// <summary>
        /// A list of supplier IDs to associate with this item.
        /// </summary>
        public ICollection<int> SupplierIds           { get; set; } = new List<int>();

        /// <summary>
        /// Indicates whether this item record is active.
        /// </summary>
        public bool IsActive                          { get; set; } = true;
    }
}
