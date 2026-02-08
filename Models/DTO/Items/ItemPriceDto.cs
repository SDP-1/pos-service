using System.ComponentModel.DataAnnotations;

namespace pos_service.Models.DTO.Items
{
    public class ItemPriceDto
    {
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MarkedPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RetailPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal WholesalePrice { get; set; }

        [Range(0, 100)]
        public decimal RetailDiscountRatio { get; set; } = 0.0m;

        [Range(0, 100)]
        public decimal WholesaleDiscountRatio { get; set; } = 0.0m;
    }
}
