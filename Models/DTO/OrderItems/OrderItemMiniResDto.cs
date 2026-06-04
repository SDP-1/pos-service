namespace pos_service.Models.DTO.OrderItems
{
    public class OrderItemMiniResDto
    {
        public int Id                        { get; set; }
        public int OrderId                   { get; set; }
        public string? OriginalItemUuid      { get; set; }
        public string PrintName              { get; set; }
        public decimal Quantity              { get; set; }
        public decimal PriceAtSale           { get; set; }
        public decimal MarkedPriceAtSale     { get; set; }
        public decimal CostAtSale            { get; set; }
        public decimal LineTotal             { get; set; }
        public bool AllowsDecimalQuantities  { get; set; }
        public bool IsReturnItem             { get; set; }
        public string? Description           { get; set; }
        public string? ReturnedOrderItemUuid { get; set; }
        public bool IsActive                 { get; set; }
    }
}
