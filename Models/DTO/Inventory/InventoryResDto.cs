using pos_service.Models.Enums;
using pos_service.Models.DTO.Items;

namespace pos_service.Models.DTO.Inventory
{
    public class InventoryResDto
    {
        public string ItemUuid                 { get; set; }
        public decimal StockQuantity           { get; set; }
        public bool AllowsDecimalQuantities    { get; set; }
        public UnitType UnitType               { get; set; }
        public List<InventoryUnitResDto> Units { get; set; } = new();

        public ItemPriceResDto Price           { get; set; } = new();

        public List<ItemExpiryResDto> Expiries { get; set; } = new();

        public string Uuid                     { get; set; }
        public DateTime CreatedAt              { get; set; }
        public DateTime? UpdatedAt             { get; set; }
        public string CreatedBy                { get; set; }
        public string? UpdatedBy               { get; set; }
        public bool IsActive                   { get; set; }
    }
}
