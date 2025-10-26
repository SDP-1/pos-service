using pos_service.Models.DTO.Audit;
using pos_service.Models.DTO.Supplier;

namespace pos_service.Models.DTO.Item
{
    public class ItemResDto : IFullResAuditDto
    {
        public int Id                         { get; set; }
        public int SubId                      { get; set; }
        public string Name                    { get; set; }
        public string PrintName               { get; set; }
        public string? BarCode                { get; set; }
        public decimal StockQuantity          { get; set; }
        public bool AllowsDecimalQuantities   { get; set; }
        public decimal MarkedPrice            { get; set; }
        public decimal RetailPrice            { get; set; }
        public decimal WholesalePrice         { get; set; }
        public decimal RetailDiscountRatio    { get; set; }
        public decimal WholesaleDiscountRatio { get; set; }
        public List<SupplierResDto> Suppliers { get; set; }


        public Guid Uuid                      { get; set; }
        public DateTime CreatedAt             { get; set; }
        public DateTime? UpdatedAt            { get; set; }
        public string CreatedBy               { get; set; }
        public string? UpdatedBy              { get; set; }
        public bool IsActive                  { get; set; }
    }
}
