using pos_service.Models.DTO.Suppliers;

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
        public decimal MarkedPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal RetailDiscountRatio { get; set; }
        public decimal WholesaleDiscountRatio { get; set; }
        public bool IsActive { get; set; }
    }
}
