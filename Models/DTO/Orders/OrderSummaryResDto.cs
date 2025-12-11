using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Orders
{
    public class OrderSummaryResDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public SaleType SaleType { get; set; }
        public int ItemCount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }

        // Basic customer info
        public string CustomerName { get; set; }
        public string CashierName { get; set; }
    }
}
