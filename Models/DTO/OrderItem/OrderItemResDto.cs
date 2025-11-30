using pos_service.Models.DTO.Audit;

namespace pos_service.Models.DTO.OrderItem
{
    public class OrderItemResDto : IFullResAuditDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? OriginalItemUuid { get; set; }
        public string ItemPrintName { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceAtSale { get; set; }
        public decimal DiscountRatioAtSale { get; set; }
        public decimal CostAtSale { get; set; }
        public decimal LineTotal { get; set; }

        // Current item information (if available)
        public string CurrentItemName { get; set; }
        public decimal CurrentPrice { get; set; }

        public bool AllowsDecimalQuantities { get; set; }

        // Audit fields
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
