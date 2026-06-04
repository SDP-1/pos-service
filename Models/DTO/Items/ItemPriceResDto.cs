using pos_service.Models.DTO.Audits;
using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    public class ItemPriceResDto : IFullResAuditDto
    {
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice            { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MarkedPrice            { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RetailPrice            { get; set; }

        [Range(0, double.MaxValue)]
        public decimal WholesalePrice         { get; set; }

        [Range(0, 100)]
        public decimal RetailDiscountRatio    { get; set; } = 0.0m;

        [Range(0, 100)]
        public decimal WholesaleDiscountRatio { get; set; } = 0.0m;

        public string Uuid                    { get; set; }
        public DateTime CreatedAt             { get; set; }
        public DateTime? UpdatedAt            { get; set; }
        public string CreatedBy               { get; set; }
        public string? UpdatedBy              { get; set; }
        public bool IsActive                  { get; set; }
    }
}
