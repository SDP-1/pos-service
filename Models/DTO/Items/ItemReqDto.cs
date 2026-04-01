using pos_service.Models.DTO.Audits;
using pos_service.Models.DTO.Inventory;
using pos_service.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    public class ItemReqDto : IReqAuditDto
    {
        // Id is optional for create requests. If not supplied (null or 0), the service will
        // assign an Id automatically (next available main Id).
        public int? Id                        { get; set; }

        // SubId is optional. If not supplied, the service will assign the next available
        // SubId for the given main Id (or 0 for a newly created main Id).
        public int? SubId                     { get; set; }

        // Backing fields to normalize values on set
        private string _name;
        private string _printName;

        [Required]
        [StringLength(200)]
        public string Name
        {
            get => _name;
            set => _name = value?.ToUpperInvariant();
        }

        [Required]
        [StringLength(40)]
        public string PrintName
        {
            get => _printName;
            set => _printName = value?.ToUpperInvariant();
        }

        [StringLength(100)]
        public string? BarCode                { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StockQuantity          { get; set; } = 0;

        public bool AllowsDecimalQuantities   { get; set; } = false;

        private UnitType? _unitType;

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

        public List<InventoryUnitDto> Units   { get; set; } = new();

        [Required]
        public ItemPriceDto Price              { get; set; }

        public ICollection<ItemExpiryDto> ExpDates { get; set; } = new List<ItemExpiryDto>();

        /// <summary>
        /// A list of supplier IDs to associate with this item.
        /// </summary>
        public ICollection<int> SupplierIds   { get; set; } = new List<int>();

        public bool IsActive                  { get; set; } = true;
    }
}
