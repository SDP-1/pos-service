using pos_service.Models.DTO.Audit;
using pos_service.Models.DTO.OrderItem;
using pos_service.Models.Enums;

namespace pos_service.Models.DTO.Order
{
    public class OrderResDto : IFullResAuditDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public SaleType SaleType { get; set; }
        public int ItemCount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }

        // Cashier information
        public int CashierId { get; set; }
        public string CashierName { get; set; }

        // Customer information (if available)
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        // Order items
        public List<OrderItemResDto> OrderItems { get; set; } = new List<OrderItemResDto>();

        // Audit fields
        public string Uuid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
