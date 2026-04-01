using pos_service.Models.DTO.Suppliers;
using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Items
{
    public class ItemMiniResDto
    {
        public int Id { get; set; }
        public int SubId { get; set; }
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string PrintName { get; set; }
        public string? BarCode { get; set; }
        public bool AllowsDecimalQuantities { get; set; }
        public UnitType UnitType { get; set; }
        public ItemPriceDto Price { get; set; }
        public List<ItemExpiryDto> ExpDates { get; set; } = new();
        public bool IsActive { get; set; }
    }
}
